using System.IO.Pipelines;
using System.Net.Sockets;
using Firejox.App.WinSocat;

namespace APPTest;

public class UnixSocketListenPiperStrategyTest
{
    class EchoPiper : IPiper
    {
        private readonly Pipe _pipe;

        public EchoPiper()
        {
            _pipe = new Pipe();
        }

        public PipeReader GetReader() => _pipe.Reader;
        public PipeWriter GetWriter() => _pipe.Writer;
        
        public void Dispose() {}
    }

    class EchoPiperFactory : IPiperFactory
    {
        public IPiper NewPiper() => new EchoPiper();
    }

    private UnixSocketListenPiperStrategy _strategy = null!;
    private CancellationTokenSource _source = null!;

    [SetUp]
    public void Init()
    {
        var element = AddressElement.TryParse("UNIX-LISTEN:tmp.sock");
        _strategy = UnixSocketListenPiperStrategy.TryParse(element);
        _source = new CancellationTokenSource();
    }

    [Test]
    public void EchoPiperTest()
    {
        var factory = new EchoPiperFactory();
        Task.Run(() => _strategy.ExecuteAsync(factory, _source.Token));
        
        while (!File.Exists("tmp.sock"))
            Thread.Sleep(50);
        
        using var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
        socket.Connect(new UnixDomainSocketEndPoint("tmp.sock"));
        using var stream = new NetworkStream(socket);

        var writer = new StreamWriter(stream);
        var reader = new StreamReader(stream);
        
        writer.WriteLine("Foo");
        writer.Flush();
        
        StringAssert.AreEqualIgnoringCase("Foo", reader.ReadLine());
    }

    [TearDown]
    public void CleanUp()
    {
        _source.Cancel();
    }
}