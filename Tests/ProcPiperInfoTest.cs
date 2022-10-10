using APP;

namespace APPTest;

public class ProcPiperInfoTest
{
    [TestCase(@"EXEC:C:\Foo.exe")]
    [TestCase(@"EXEC:C:\Foo.exe bar")]
    [TestCase(@"EXEC:'C:\Foo.exe bar'")]
    public void ValidInputParseTest(string input)
    {
        var element = AddressElement.TryParse(input);
        Assert.NotNull(APP.ProcPiperInfo.TryParse(element));
    }

    [TestCase(@"EXEC:C:\Foo.exe bar")]
    [TestCase(@"exec:C:\Foo.exe bar")]
    public void CaseInsensitiveValidInputParseTest(string input)
    {
        var element = AddressElement.TryParse(input);
        Assert.NotNull(APP.ProcPiperInfo.TryParse(element));
    }

    [TestCase("STDIO")]
    [TestCase("TCP:127.0.0.1:80")]
    [TestCase("TCP-LISTEN:127.0.0.1:80")]
    [TestCase("NPIPE:fooServer:barPipe")]
    [TestCase("NPIPE-LISTEN:fooPipe")]
    public void InvalidInputParseTest(string input)
    {
        var element = AddressElement.TryParse(input);
        Assert.Null(APP.ProcPiperInfo.TryParse(element));
    }

    [TestCase(@"EXEC:C:\Foo.exe bar", ExpectedResult = @"C:\Foo.exe")]
    [TestCase(@"EXEC:C:\Foo.exe", ExpectedResult = @"C:\Foo.exe")]
    public string FileNamePatternMatchTest(string input)
    {
        var element = AddressElement.TryParse(input);
        return APP.ProcPiperInfo.TryParse(element).FileName;
    }

    [TestCase(@"EXEC:C:\Foo.exe bar1 bar2", ExpectedResult = "bar1 bar2")]
    public string ArgumentPatternMatchTest(string input)
    {
        var element = AddressElement.TryParse(input);
        return APP.ProcPiperInfo.TryParse(element).Arguments;
    }
}