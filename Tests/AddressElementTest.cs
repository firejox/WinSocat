using APP;

namespace APPTest;

public class AddressElementTest
{
    [TestCase("tag")]
    [TestCase("tag:address")]
    [TestCase("tag,opt1, opt2")]
    [TestCase("tag:address,opt1,opt2")]
    [TestCase(@"tag:'foo bar'")]
    [TestCase("tag:\"foo bar\",opt1,opt2")]
    [TestCase("tag:\'foo \"bar\"\',opt1,opt2")]
    public void ValidInputParseTest(string input)
    {
        Assert.NotNull(AddressElement.TryParse(input));
    }

    [TestCase("tag:\'foo\"")]
    [TestCase("tag:\"foo bar\'")]
    [TestCase("tag:\'foo \"bar\',opt1, opt2")]
    public void InvalidInputParseTest(string input)
    {
        Assert.Null(AddressElement.TryParse(input));
    }

    [TestCase("STDIO", ExpectedResult = "STDIO")]
    [TestCase("TCP:localhost:80", ExpectedResult = "TCP")]
    [TestCase("TCP-LISTEN:127.0.0.1:80", ExpectedResult = "TCP-LISTEN")]
    [TestCase("NPIPE::fooPipe", ExpectedResult = "NPIPE")]
    [TestCase(@"EXEC:C:\foo.exe", ExpectedResult = "EXEC")]
    [TestCase("WSL:\'echo \"Hello World\"\',distribution=Ubuntu,user=root", ExpectedResult = "WSL")]
    public string TagParseCheck(string input)
    {
        return AddressElement.TryParse(input).Tag;
    }

    [TestCase("STDIO", ExpectedResult = "")]
    [TestCase("TCP:localhost:80", ExpectedResult = "localhost:80")]
    [TestCase("TCP-LISTEN:127.0.0.1:80", ExpectedResult = "127.0.0.1:80")]
    [TestCase("NPIPE::fooPipe", ExpectedResult = ":fooPipe")]
    [TestCase(@"EXEC:C:\foo.exe", ExpectedResult = @"C:\foo.exe")]
    [TestCase("WSL:\'echo \"Hello World\"\',distribution=Ubuntu,user=root", ExpectedResult = "\'echo \"Hello World\"\'")]
    public string AddressParseCheck(string input)
    {
        return AddressElement.TryParse(input).Address;
    }
    
}