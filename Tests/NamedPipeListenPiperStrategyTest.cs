using System.IO.Pipelines;
using System.IO.Pipes;
using Firejox.App.WinSocat;

namespace APPTest;

public class NamedPipeListenPiperStrategyTest
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

    class EchoPiperFactory : IPiperFactory
    {
        public IPiper NewPiper() => new EchoPiper();
    }

    private NamedPipeListenPiperStrategy _strategy;
    private CancellationTokenSource _source;

    [SetUp]
    public void Init()
    {
        var element = AddressElement.TryParse("NPIPE-LISTEN:dummyPipe");
        _strategy = NamedPipeListenPiperStrategy.TryParse(element);
        _source = new CancellationTokenSource();
    }

    [Test]
    public void EchoPiperTest()
    {
        var factory = new EchoPiperFactory();
        var task = Task.Run(() => _strategy.ExecuteAsync(factory, _source.Token));

        using (var clientStream = new NamedPipeClientStream(".", "dummyPipe", PipeDirection.InOut))
        {
            clientStream.Connect();

            var reader = new StreamReader(clientStream);
            var writer = new StreamWriter(clientStream);
            
            writer.WriteLine("Foo");
            writer.Flush();
            
            StringAssert.AreEqualIgnoringCase("Foo", reader.ReadLine());
        }
    }

    [TearDown]
    public void CleanUp()
    {
        _source.Cancel();
    }
}