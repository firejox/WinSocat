using Firejox.App.WinSocat;

namespace APPTest;

public class NamedPipeListenPiperInfoTest
{
    [TestCase("NPIPE-LISTEN:fooPipe")]
    public void VaildInputParseTest(string input)
    {
        var element = AddressElement.TryParse(input);
        Assert.NotNull(Firejox.App.WinSocat.NamedPipeListenPiperInfo.TryParse(element));
    }

    [TestCase("NPIPE-LISTEN:fooPipe")]
    [TestCase("npipe-listen:fooPipe")]
    public void CaseInsensitiveValidInputParseTest(string input)
    {
        var element = AddressElement.TryParse(input);
        Assert.NotNull(Firejox.App.WinSocat.NamedPipeListenPiperInfo.TryParse(element));
    }
    
    [TestCase("STDIO")]
    [TestCase("TCP:127.0.0.1:80")]
    [TestCase("TCP-LISTEN:127.0.0.1:80")]
    [TestCase("NPIPE:fooServer:barPipe")]
    [TestCase(@"EXEC:'C:\Foo.exe bar'")]
    public void InvalidInputParseTest(string input)
    {
        var element = AddressElement.TryParse(input);
        Assert.Null(Firejox.App.WinSocat.NamedPipeListenPiperInfo.TryParse(element));
    }

    [TestCase("NPIPE-LISTEN:fooPipe", ExpectedResult = "fooPipe")]
    public string PipePatternMatchTest(string input)
    {
        var element = AddressElement.TryParse(input);
        return Firejox.App.WinSocat.NamedPipeListenPiperInfo.TryParse(element).PipeName;
    }
}