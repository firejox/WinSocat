using APP;

namespace APPTest;

public class StdPiperInfoTest
{

    [TestCase("STDIO")]
    public void VaildInputCheckTest(string input)
    {
        var element = AddressElement.TryParse(input);
        Assert.IsTrue(APP.StdPiperInfo.Check(element));
    }

    [TestCase("STDIO")]
    [TestCase("stdio")]
    public void CaseInsensitiveValidInputCheckTest(string input)
    {
        var element = AddressElement.TryParse(input);
        Assert.IsTrue(APP.StdPiperInfo.Check(element));
    }

    [TestCase("TCP:127.0.0.1:80")]
    [TestCase("TCP-LISTEN:127.0.0.1:80")]
    [TestCase("NPIPE:fooServer:barPipe")]
    [TestCase("NPIPE-LISTEN:fooPipe")]
    [TestCase(@"EXEC:'C:\Foo.exe bar'")]
    public void InvalidInputCheckTest(string input)
    {
        var element = AddressElement.TryParse(input);
        Assert.IsFalse(APP.StdPiperInfo.Check(element));
    }
}