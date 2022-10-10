using APP;

using System.IO.Pipelines;
using System.Net.Sockets;

namespace APPTest;

public class TcpListenPiperTest
{
    class EchoPiper : IPiper
    {
        private Pipe _pipe;

        public EchoPiper()
        {
            _pipe = new Pipe();
        }

        public PipeReader GetReader() => _pipe.Reader;
        public PipeWriter GetWriter() => _pipe.Writer;
        
        public void Dispose() {}
    }

    class EchoPiperFactory : PiperFactory
    {
        public IPiper NewPiper() => new EchoPiper();
    }

    private TcpListenPiperStrategy _strategy = null!;
    private CancellationTokenSource _source = null!;

    [SetUp]
    public void Init()
    {
        var element = AddressElement.TryParse("TCP-LISTEN:127.0.0.1:10000");
        _strategy = TcpListenPiperStrategy.TryParse(element);
        _source = new CancellationTokenSource();
        
    }
    
    [Test]
    public void EchoPiperTest()
    {
        var factory = new EchoPiperFactory();
        Task.Run(() => _strategy.ExecuteAsync(factory, _source.Token));
        using var client = new TcpClient("127.0.0.1", 10000);
        using var stream = client.GetStream();
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