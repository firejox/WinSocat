using Firejox.App.WinSocat;

namespace APPTest;

public class UnixSocketListenPiperInfoTest
{
    [TestCase("UNIX-LISTEN:foo.sock")]
    [TestCase(@"UNIX-LISTEN:C\foo.sock")]
    public void ValidInputParseTest(string input)
    {
        var element = AddressElement.TryParse(input);
        Assert.NotNull(UnixSocketListenPiperInfo.TryParse(element));
    }

    [TestCase("UNIX-LISTEN:foo.sock")]
    [TestCase(@"unix-listen:C:\foo.sock")]
    public void CaseInsensitiveValidInputParseTest(string input)
    {
        var element = AddressElement.TryParse(input);
        Assert.NotNull(UnixSocketListenPiperInfo.TryParse(element));
    }

    [TestCase("STDIO")]
    [TestCase("TCP:127.0.0.1:80")]
    [TestCase("TCP-LISTEN:127.0.0.1:80")]
    [TestCase("NPIPE:fooServer:barPipe")] 
    [TestCase("NPIPE-LISTEN:fooPipe")] 
    [TestCase(@"EXEC:'C:\Foo.exe bar'")]
    [TestCase("UNIX:foo.sock")]
    public void InvalidInputParseTest(string input)
    {
        var element = AddressElement.TryParse(input);
        Assert.Null(UnixSocketListenPiperInfo.TryParse(element));
    }
    
    [TestCase("UNIX-LISTEN:foo.sock", ExpectedResult = "foo.sock")]
    [TestCase(@"UNIX-LISTEN:D:\bar.sock", ExpectedResult = @"D:\bar.sock")]
    public string PathPatternMatchTest(string input)
    {
        var element = AddressElement.TryParse(input);
        return UnixSocketListenPiperInfo.TryParse(element).EndPoint.ToString()!;
    }
}