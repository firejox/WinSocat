using System.Net;
using Firejox.App.WinSocat;

namespace APPTest;

public class TcpListenPiperInfoTest
{
    [TestCase("TCP-LISTEN:127.0.0.1:80")]
    [TestCase("TCP-LISTEN::80")]
    [TestCase("TCP-LISTEN:80")]
    public void ValidInputParseTest(string input)
    {
        var element = AddressElement.TryParse(input);
        Assert.NotNull(Firejox.App.WinSocat.TcpListenPiperInfo.TryParse(element));
    }

    [TestCase("TCP-LISTEN:127.0.0.1:80")]
    [TestCase("tcp-listen:127.0.0.1:80")]
    public void CaseInsensitiveValidInputParseTest(string input)
    {
        var element = AddressElement.TryParse(input);
        Assert.NotNull(Firejox.App.WinSocat.TcpListenPiperInfo.TryParse(element));
    }

    [TestCase("STDIO")]
    [TestCase("TCP:127.0.0.1:80")]
    [TestCase("NPIPE:fooServer:barPipe")]
    [TestCase("NPIPE-LISTEN:fooPipe")]
    [TestCase(@"EXEC:'C:\Foo.exe bar'")]
    public void InvalidInputParseTest(string input)
    {
        var element = AddressElement.TryParse(input);
        Assert.Null(Firejox.App.WinSocat.TcpListenPiperInfo.TryParse(element));
    }

    [TestCaseSource(nameof(AddressCases))]
    public void AddressPatternMatchParseTest(string input, IPAddress expect)
    {
        var element = AddressElement.TryParse(input);
        Assert.That(Firejox.App.WinSocat.TcpListenPiperInfo.TryParse(element).Address, Is.EqualTo(expect));
    }

    private static object[] AddressCases =
    {
        new object[] { "TCP-LISTEN:127.0.0.1:80", IPAddress.Parse("127.0.0.1") },
        new object[] { "TCP-LISTEN::80", IPAddress.Any },
        new object[] { "TCP-LISTEN:80", IPAddress.Any },
    };

    [TestCase("TCP-LISTEN:127.0.0.1:80", ExpectedResult = 80)]
    [TestCase("TCP-LISTEN::80", ExpectedResult = 80)]
    [TestCase("TCP-LISTEN:80", ExpectedResult = 80)]
    public int PortPatternMatchTest(string input)
    {
        var element = AddressElement.TryParse(input);
        return Firejox.App.WinSocat.TcpListenPiperInfo.TryParse(element).Port;
    }
}