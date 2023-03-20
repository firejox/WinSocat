using Firejox.App.WinSocat;

namespace APPTest;

public class UnixSocketStreamPiperInfoTest
{
    [TestCase("UNIX:foo.sock")]
    [TestCase(@"UNIX:C:\local.sock")]
    public void ValidInputParseTest(string input)
    {
        var element = AddressElement.TryParse(input);
        Assert.NotNull(UnixSocketStreamPiperInfo.TryParse(element));
    }

    [TestCase("UNIX:foo.sock")]
    [TestCase("unix:foo.sock")]
    public void CaseInsensitiveValidInputParseTest(string input)
    {
        var element = AddressElement.TryParse(input);
        Assert.NotNull(UnixSocketStreamPiperInfo.TryParse(element));
    }

    [TestCase("STDIO")]
    [TestCase("TCP:127.0.0.1:80")]
    [TestCase("TCP-LISTEN:127.0.0.1:80")]
    [TestCase("NPIPE:fooServer:barPipe")] 
    [TestCase("NPIPE-LISTEN:fooPipe")] 
    [TestCase(@"EXEC:'C:\Foo.exe bar'")]
    public void InvalidInputParse(string input)
    {
        var element = AddressElement.TryParse(input);
        Assert.Null(UnixSocketStreamPiperInfo.TryParse(element));
    }

    [TestCase("UNIX:foo.sock", ExpectedResult = "foo.sock")]
    [TestCase(@"UNIX:D:\bar.sock", ExpectedResult = @"D:\bar.sock")]
    public string PathPatternMatchTest(string input)
    {
        var element = AddressElement.TryParse(input);
        return UnixSocketStreamPiperInfo.TryParse(element).EndPoint.ToString()!;
    }
}