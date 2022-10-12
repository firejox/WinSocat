using System.Net.Sockets;
using System.Net;

namespace Firejox.App.WinSocat;

public class TcpStreamPiperInfo
{
    private readonly string _host;
    private readonly int _port;

    public string Host => _host;
    public int Port => _port;

    public TcpStreamPiperInfo(string host, int port)
    {
        _host = host;
        _port = port;
    }

    public static TcpStreamPiperInfo TryParse(AddressElement element)
    {
        if (!element.Tag.Equals("TCP", StringComparison.OrdinalIgnoreCase)) return null!;
        
        string host;
        int sepIndex = element.Address.LastIndexOf(':');

        if (sepIndex == -1 || sepIndex == 0)
            host = "0.0.0.0";
        else
            host = element.Address.Substring(0, sepIndex);
            
        int port = Int32.Parse(element.Address.Substring(sepIndex + 1));

        return new TcpStreamPiperInfo(host, port);
    }
}

public class TcpListenPiperInfo
{
    private readonly IPAddress _address;
    private readonly int _port;

    public IPAddress Address => _address;
    public int Port => _port;

    public TcpListenPiperInfo(IPAddress address, int port)
    {
        _address = address;
        _port = port;
    }
    
    public static TcpListenPiperInfo TryParse(AddressElement element)
    {
        if (element.Tag.Equals("TCP-LISTEN", StringComparison.OrdinalIgnoreCase))
        {
            IPAddress address;
            int sepIndex = element.Address.LastIndexOf(':');
            
            if (sepIndex == -1 || sepIndex == 0)
                address = IPAddress.Any;
            else
                address = IPAddress.Parse(element.Address.Substring(0, sepIndex));
            
            int port = Int32.Parse(element.Address.Substring(sepIndex + 1));
            return new TcpListenPiperInfo(address, port);
        }

        return null!;
    }
}

public class TcpListenPiper : IListenPiper
{
    private TcpListener _server;

    public TcpListenPiper(TcpListener server)
    {
        _server = server;
        _server.Start();
    }
    
    public TcpListenPiper(IPAddress address, int port) : this(new TcpListener(address, port)) {}

    public IPiper NewIncomingPiper()
    {
        return new TcpStreamPiper(_server.AcceptTcpClient());
    }

    public async Task<IPiper> NewIncomingPiperAsync()
    {
        var client = await _server.AcceptTcpClientAsync();
        return new TcpStreamPiper(client);
    }

    public void Close()
    {
        _server.Stop();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        try
        {
            if (disposing && _server != null)
                _server.Stop();
        }
        finally
        {
            _server = null!;
        }
    }
}


public class TcpListenPiperStrategy : ListenPiperStrategy
{
    private readonly TcpListenPiperInfo _info;
    public TcpListenPiperInfo Info => _info;

    public TcpListenPiperStrategy(TcpListenPiperInfo info)
    {
        _info = info;
    }

    protected override IListenPiper NewListenPiper()
    {
        return new TcpListenPiper(_info.Address, _info.Port);
    }

    public static TcpListenPiperStrategy TryParse(AddressElement element)
    {
        TcpListenPiperInfo info;
        
        if ((info = TcpListenPiperInfo.TryParse(element)) != null)
            return new TcpListenPiperStrategy(info);

        return null!;
    }
}

public class TcpStreamPiperStrategy : PiperStrategy
{
    private readonly TcpStreamPiperInfo _info;
    public TcpStreamPiperInfo Info => _info;

    public TcpStreamPiperStrategy(TcpStreamPiperInfo info)
    {
        _info = info;
    }

    protected override IPiper NewPiper()
    {
        return new TcpStreamPiper(_info.Host, _info.Port);
    }

    public static TcpStreamPiperStrategy TryParse(AddressElement element)
    {
        TcpStreamPiperInfo info;

        if ((info = TcpStreamPiperInfo.TryParse(element)) != null)
            return new TcpStreamPiperStrategy(info);

        return null!;
    }
}

public class TcpStreamPiper : StreamPiper
{
    private TcpClient _client;

    public TcpStreamPiper(TcpClient client) : base(client.GetStream())
    {
        _client = client;
    }
    
    public TcpStreamPiper(string host, int port) : this(new TcpClient(host, port)) {}

    protected override void Dispose(bool disposing)
    {
        try
        {
            base.Dispose(disposing);
            if (disposing && _client != null)
            {
                _client.Dispose();
            }
        }
        finally
        {
            _client = null!;
        }
    }
}

public class TcpStreamPiperFactory : IPiperFactory
{
    private readonly TcpStreamPiperInfo _info;
    public TcpStreamPiperInfo Info => _info;

    public TcpStreamPiperFactory(TcpStreamPiperInfo info)
    {
        _info = info;
    }

    public IPiper NewPiper()
    {
        return new TcpStreamPiper(_info.Host, _info.Port);
    }

    public static TcpStreamPiperFactory TryParse(AddressElement element)
    {
        TcpStreamPiperInfo info;

        if ((info = TcpStreamPiperInfo.TryParse(element)) != null)
            return new TcpStreamPiperFactory(info);

        return null!;
    }
}