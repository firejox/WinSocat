using Firejox.App.WinSocat;
using System.IO.Pipelines;
using System.IO.Pipes;
using System.Net;
using System.Net.Sockets;

namespace APPTest;


public class PiperStrategyTest
{
    public class DummyIOPiper : IPiper
    {
        private Pipe _readPipe;
        private MemoryStream _writeStream;
        private PipeWriter _writer;
        
        public DummyIOPiper(Pipe readPipe, MemoryStream writeStream)
        {
            _readPipe = readPipe;
            _writeStream = writeStream;
            _writer = PipeWriter.Create(_writeStream);
        }

        public PipeReader GetReader()
        {
            return _readPipe.Reader;
        }

        public PipeWriter GetWriter()
        {
            return _writer;
        }

        public void Dispose()
        {
            
        }
    }

    public class EchoPiper : IPiper
    {
        private Pipe _pipe;

        public EchoPiper(Pipe pipe)
        {
            _pipe = pipe;
        }

        public PipeReader GetReader() => _pipe.Reader;
        public PipeWriter GetWriter() => _pipe.Writer;
        
        public void Dispose() {}
    }

    public class EchoPiperFactory : IPiperFactory
    {
        public IPiper NewPiper() => new EchoPiper(new Pipe());
    }

    public class DummyIOPiperStrategy : PiperStrategy
    {
        private Pipe _readPipe;
        private MemoryStream _writeStream;

        public DummyIOPiperStrategy(Pipe readPipe, MemoryStream writeStream)
        {
            _readPipe = readPipe;
            _writeStream = writeStream;
        }

        protected override IPiper NewPiper() => new DummyIOPiper(_readPipe, _writeStream);
    }

    private Pipe _readPipe;
    private MemoryStream _writeStream;
    private DummyIOPiperStrategy _strategy;
    private StreamWriter _writer;

    [SetUp]
    public void Init()
    {
        _readPipe = new Pipe();
        _writeStream = new MemoryStream(100);
        _strategy = new DummyIOPiperStrategy(_readPipe, _writeStream);
        _writer = new StreamWriter(_readPipe.Writer.AsStream());
    }

    [Test]
    public void EchoPiperTest()
    {
        var factory = new EchoPiperFactory();

        Task.Run(async () =>
        {
            _writer.Write("Foo");
            _writer.Flush();
            await Task.Delay(30);
            await _readPipe.Writer.CompleteAsync();
        });

        _strategy.Execute(factory);
        
        _writeStream.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(_writeStream);
        
        StringAssert.AreEqualIgnoringCase("Foo", reader.ReadToEnd());
    }

    [Test]
    public void TcpStreamPiperTest()
    {
        var tcs = new TaskCompletionSource<string>();
        
        var serverThread = new Thread(() =>
        {
            var server = new TcpListener(IPAddress.Parse("127.0.0.1"), 10000);
            server.Start();

            using (var client = server.AcceptTcpClient())
            {
                using (var stream = client.GetStream())
                {
                    var reader = new StreamReader(stream);
                    var writer = new StreamWriter(stream);
                    
                    writer.Write("Bar");
                    writer.Flush();

                    tcs.SetResult(reader.ReadToEnd());
                }
            }
            server.Stop();
        });
        
        serverThread.Start();

        var factory = Firejox.App.WinSocat.TcpStreamPiperFactory.TryParse(AddressElement.TryParse("TCP:127.0.0.1:10000"));
        
        Task.Run(async () =>
        {
            await _writer.WriteAsync("Foo");
            await _writer.FlushAsync();
            await Task.Delay(100);
            await _readPipe.Writer.CompleteAsync();
        });
        
        _strategy.Execute(factory);
        serverThread.Join();

        _writeStream.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(_writeStream);
        
        StringAssert.AreEqualIgnoringCase("Foo", tcs.Task.Result);
        StringAssert.AreEqualIgnoringCase("Bar", reader.ReadToEnd());
    }

    [Test]
    public void NamedPipeStreamPiperTest()
    {
        var tcs = new TaskCompletionSource<string>();
        var serverThread = new Thread(() =>
        {
            using (var serverStream = new NamedPipeServerStream("PipeDemo", PipeDirection.InOut, -1))
            {
                serverStream.WaitForConnection();
                var reader = new StreamReader(serverStream);
                var writer = new StreamWriter(serverStream);
                writer.Write("Bar");
                writer.Flush();
                tcs.SetResult(reader.ReadToEnd());
            }
        });
        
        serverThread.Start();
        
        var factory = NamedPipeStreamPiperFactory.TryParse(AddressElement.TryParse("NPIPE:PipeDemo"));
        
        var writeTask = Task.Run(async () =>
        {
            await _writer.WriteAsync("Foo");
            await _writer.FlushAsync();
            await Task.Delay(100);
            await _readPipe.Writer.CompleteAsync();
        });
        
        _strategy.Execute(factory);

        _writeStream.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(_writeStream);
        StringAssert.AreEqualIgnoringCase("Bar", reader.ReadToEnd());
        StringAssert.AreEqualIgnoringCase("Foo", tcs.Task.Result);
    }

    [Test]
    public void UnixSocketStreamPiperTest()
    {
        var tcs = new TaskCompletionSource<string>();
        var serverThread = new Thread(() =>
        {
            try
            {
                using (var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified))
                {
                    socket.Bind(new UnixDomainSocketEndPoint("tmp.sock"));
                    socket.Listen();
                    using (var stream = new NetworkStream(socket.Accept(), true))
                    {
                        var reader = new StreamReader(stream);
                        var writer = new StreamWriter(stream);
                        
                        writer.Write("Bar");
                        writer.Flush();
                        
                        tcs.SetResult(reader.ReadLine());
                    }
                }
            }
            finally
            {
                if (File.Exists("tmp.sock"))
                    File.Delete("tmp.sock");
            }
        });
        
        serverThread.Start();
        
        while (!File.Exists("tmp.sock"))
            Thread.Sleep(50);

        var factory = UnixSocketStreamPiperFactory.TryParse(AddressElement.TryParse("UNIX:tmp.sock"));
        
        var writeTask = Task.Run(async () =>
        {
            await _writer.WriteLineAsync("Foo");
            await _writer.FlushAsync();
            await Task.Delay(100);
            await _readPipe.Writer.CompleteAsync();
        });
        
        _strategy.Execute(factory);
        
        _writeStream.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(_writeStream);
        StringAssert.AreEqualIgnoringCase("Bar", reader.ReadToEnd());
        StringAssert.AreEqualIgnoringCase("Foo", tcs.Task.Result);
    }

    [TearDown]
    public void CleanUp()
    {
        _writeStream.Dispose();
    }
}