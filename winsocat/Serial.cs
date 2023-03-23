using System.IO.Ports;

namespace Firejox.App.WinSocat;

public class SerialPortPiperInfo
{
    private readonly string _portName;
    public string PortName => _portName;
    
    private readonly int _baudRate;
    public int BaudRate => _baudRate;
    
    private readonly Parity _parity;
    public Parity Partiy => _parity;
    
    private readonly int _dataBits;
    public int DataBits => _dataBits;
    
    private readonly StopBits _stopBits;
    public StopBits StopBits => _stopBits;

    public SerialPortPiperInfo(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
    {
        _portName = portName;
        _baudRate = baudRate;
        _parity = parity;
        _dataBits = dataBits;
        _stopBits = stopBits;
    }

    public static SerialPortPiperInfo TryParse(AddressElement element)
    {
        if (!element.Tag.Equals("SP", StringComparison.OrdinalIgnoreCase))
            return null!;

        string portName = element.Address;
        int baudRate;
        Parity parity;
        int dataBits;
        StopBits stopBits;

        if (!element.Options.TryGetValue("baudrate", out var tmp))
            baudRate = 9600;
        else if (!Int32.TryParse(tmp, out baudRate))
            return null!;

        if (!element.Options.TryGetValue("parity", out tmp!))
            parity = Parity.None;
        else if (!Enum.TryParse(tmp, out parity))
            return null!;

        if (!element.Options.TryGetValue("databits", out tmp!))
            dataBits = 8;
        else if (!Int32.TryParse(tmp, out dataBits))
            return null!;

        if (!element.Options.TryGetValue("stopbits", out tmp!))
            stopBits = StopBits.One;
        else if (!Enum.TryParse(tmp, out stopBits))
            return null!;
        return new SerialPortPiperInfo(portName, baudRate, parity, dataBits, stopBits);
    }
}

public class SerialPortPiper : StreamPiper
{
    private SerialPort _serialPort;

    public SerialPortPiper(SerialPort serialPort) : base(OpenAndGet(serialPort))
    {
        _serialPort = serialPort;
    }

    protected override void Dispose(bool disposing)
    {
        try
        {
            base.Dispose(disposing);
            if (disposing && _serialPort is not null)
            {
                _serialPort.Dispose();
            }
        }
        finally
        {
            _serialPort = null!;
        }
    }

    private static Stream OpenAndGet(SerialPort serialPort)
    {
        if (!serialPort.IsOpen)
            serialPort.Open();
        return serialPort.BaseStream;
    }
}

public class SerialPortPiperFactory : IPiperFactory
{
    private readonly SerialPortPiperInfo _info;

    public SerialPortPiperFactory(SerialPortPiperInfo info)
    {
        _info = info;
    }

    public IPiper NewPiper()
    {
        return new SerialPortPiper(
            new SerialPort(
                _info.PortName, 
                _info.BaudRate, 
                _info.Partiy, 
                _info.DataBits, 
                _info.StopBits
                )
            );
    }

    public static SerialPortPiperFactory TryParse(AddressElement element)
    {
        SerialPortPiperInfo info;
        if ((info = SerialPortPiperInfo.TryParse(element)) is not null)
            return new SerialPortPiperFactory(info);
        return null!;
    }
}

public class SerialPortPiperStrategy : PiperStrategy
{
    private readonly SerialPortPiperInfo _info;

    public SerialPortPiperStrategy(SerialPortPiperInfo info)
    {
        _info = info;
    }
    
    protected override IPiper NewPiper()
    {
        return new SerialPortPiper(
            new SerialPort(
                _info.PortName, 
                _info.BaudRate, 
                _info.Partiy, 
                _info.DataBits, 
                _info.StopBits
                )
            );
    }

    public static SerialPortPiperStrategy TryParse(AddressElement element)
    {
        SerialPortPiperInfo info;

        if ((info = SerialPortPiperInfo.TryParse(element)) is not null)
            return new SerialPortPiperStrategy(info);

        return null!;
    }
}