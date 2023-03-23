using System.IO.Ports;
using Firejox.App.WinSocat;

namespace APPTest;

public class SerialPortPiperInfoTest
{

    [TestCase("SP:COM1")]
    [TestCase("SP:COM2,baudrate=12500,parity=1,databits=16,stopbits=0")]
    public void ValidInputParseTest(string input)
    {
        var element = AddressElement.TryParse(input);
        Assert.NotNull(SerialPortPiperInfo.TryParse(element));
    }

    [TestCase("sp:COM1")]
    [TestCase("sp:COM2,baudrate=12500,parity=1,databits=16,stopbits=0")]
    public void CaseInsensitiveValidInputParseTest(string input)
    {
        var element = AddressElement.TryParse(input);
        Assert.NotNull(SerialPortPiperInfo.TryParse(element));
    }

    [TestCase("STDIO")]
    [TestCase("TCP:127.0.0.1:80")]
    [TestCase("TCP-LISTEN:127.0.0.1:80")]
    [TestCase("NPIPE:fooServer:barPipe")] 
    [TestCase("NPIPE-LISTEN:fooPipe")] 
    [TestCase(@"EXEC:'C:\Foo.exe bar'")]
    [TestCase("UNIX:foo.sock")]
    [TestCase("UNIX-LISTEN:foo.sock")]
    public void InvalidInputParseTest(string input)
    {
        var element = AddressElement.TryParse(input);
        Assert.Null(SerialPortPiperInfo.TryParse(element));
    }

    [TestCase("sp:COM1", ExpectedResult = "COM1")]
    [TestCase("sp:COM2,baudrate=12500,parity=1,databits=16,stopbits=0", ExpectedResult = "COM2")]
    public string PortNamePatternParseTest(string input)
    {
        var element = AddressElement.TryParse(input);
        return SerialPortPiperInfo.TryParse(element).PortName;
    }
    
    [TestCase("sp:COM1", ExpectedResult = 9600)]
    [TestCase("sp:COM2,baudrate=12500,parity=1,databits=16,stopbits=0", ExpectedResult = 12500)]
    public int BaudRatePatternParseTest(string input)
    {
        var element = AddressElement.TryParse(input);
        return SerialPortPiperInfo.TryParse(element).BaudRate;
    }

    [TestCase("sp:COM1", ExpectedResult = Parity.None)]
    [TestCase("sp:COM2,baudrate=12500,parity=1,databits=16,stopbits=0", ExpectedResult = Parity.Odd)]
    public Parity PartityPatternParseTest(string input)
    {
        var element = AddressElement.TryParse(input);
        return SerialPortPiperInfo.TryParse(element).Partiy;
    }

    [TestCase("sp:COM1", ExpectedResult = 8)]
    [TestCase("sp:COM2,baudrate=12500,parity=1,databits=16,stopbits=0", ExpectedResult = 16)]
    public int DataBitsPatternParseTest(string input)
    {
        var element = AddressElement.TryParse(input);
        return SerialPortPiperInfo.TryParse(element).DataBits;
    }

    [TestCase("sp:COM1", ExpectedResult = StopBits.One)]
    [TestCase("sp:COM2,baudrate=12500,parity=1,databits=16,stopbits=0", ExpectedResult = StopBits.None)]
    public StopBits StopBitsPatternParseTest(string input)
    {
        var element = AddressElement.TryParse(input);
        return SerialPortPiperInfo.TryParse(element).StopBits;
    }
}