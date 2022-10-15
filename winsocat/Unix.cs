using System.Net.Sockets;

namespace Firejox.App.WinSocat;

public class UnixSocketStreamPiperInfo
{
    private readonly UnixDomainSocketEndPoint _endPoint;
    public UnixDomainSocketEndPoint EndPoint => _endPoint;

    public UnixSocketStreamPiperInfo(UnixDomainSocketEndPoint endPoint)
    {
        _endPoint = endPoint;
    }

    public static UnixSocketStreamPiperInfo TryParse(AddressElement element)
    {
        if (!element.Tag.Equals("UNIX", StringComparison.OrdinalIgnoreCase)) return null!;
        return new UnixSocketStreamPiperInfo(new UnixDomainSocketEndPoint(element.Address));
    }
}

public class UnixSocketListenPiperInfo
{
    private readonly UnixDomainSocketEndPoint _endPoint;
    public UnixDomainSocketEndPoint EndPoint => _endPoint;

    public UnixSocketListenPiperInfo(UnixDomainSocketEndPoint endPoint)
    {
        _endPoint = endPoint;
    }

    public static UnixSocketListenPiperInfo TryParse(AddressElement element)
    {
        if (!element.Tag.Equals("UNIX-LISTEN", StringComparison.OrdinalIgnoreCase)) return null;
        return new UnixSocketListenPiperInfo(new UnixDomainSocketEndPoint(element.Address));
    }
}

public class UnixSocketStreamPiper : StreamPiper
{
    private Socket _socket;

    public UnixSocketStreamPiper(Socket socket) : base(new NetworkStream(socket))
    {
        _socket = socket;
    }

    public UnixSocketStreamPiper(UnixDomainSocketEndPoint endPoint) : this(ConnectedSocket(endPoint))
    { }

    protected override void Dispose(bool disposing)
    {
        try
        {
            base.Dispose(disposing);
            if (disposing && _socket is not null)
            {
                _socket.Dispose();
            }
        }
        finally
        {
            _socket = null!;
        }
    }

    private static Socket ConnectedSocket(UnixDomainSocketEndPoint endPoint)
    {
        var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
        socket.Connect(endPoint);
        return socket;
    }
}

public class UnixSocketListenPiper : IListenPiper
{
    private UnixDomainSocketEndPoint _endPoint;
    private Socket _socket;

    public UnixSocketListenPiper(UnixDomainSocketEndPoint endPoint)
    {
        _endPoint = endPoint;
        _socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
        _socket.Bind(_endPoint);
        _socket.Listen();
    }
    
    public IPiper NewIncomingPiper()
    {
        return new UnixSocketStreamPiper(_socket.Accept());
    }

    public async Task<IPiper> NewIncomingPiperAsync()
    {
        var socket = await _socket.AcceptAsync();
        return new UnixSocketStreamPiper(socket);
    }

    public void Close()
    {
        _socket.Close();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        string path;
        try
        {
            if (disposing)
            {
                if (_socket is not null)
                    _socket.Dispose();
                if (_endPoint is not null)
                {
                    path = _endPoint.ToString() ?? "";
                    if (File.Exists(path))
                        File.Delete(path);
                }
            }
        }
        finally
        {
            _socket = null!;
            _endPoint = null!;
        }
    }
}

public class UnixSocketStreamPiperStrategy : PiperStrategy
{
    private UnixSocketStreamPiperInfo _info;

    public UnixSocketStreamPiperInfo Info => _info;

    public UnixSocketStreamPiperStrategy(UnixSocketStreamPiperInfo info)
    {
        _info = info;
    }
    
    protected override IPiper NewPiper()
    {
        return new UnixSocketStreamPiper(_info.EndPoint);
    }

    public static UnixSocketStreamPiperStrategy TryParse(AddressElement element)
    {
        UnixSocketStreamPiperInfo info;
        if ((info = UnixSocketStreamPiperInfo.TryParse(element)) is not null)
            return new UnixSocketStreamPiperStrategy(info);
        
        return null!;
    }
}

public class UnixSocketListenPiperStrategy : ListenPiperStrategy
{
    private readonly UnixSocketListenPiperInfo _info;

    public UnixSocketListenPiperStrategy(UnixSocketListenPiperInfo info)
    {
        _info = info;
    }

    protected override IListenPiper NewListenPiper()
    {
        return new UnixSocketListenPiper(_info.EndPoint);
    }

    public static UnixSocketListenPiperStrategy TryParse(AddressElement element)
    {
        UnixSocketListenPiperInfo info;

        if ((info = UnixSocketListenPiperInfo.TryParse(element)) is not null)
            return new UnixSocketListenPiperStrategy(info);

        return null!;
    }
}

public class UnixSocketStreamPiperFactory : IPiperFactory
{
    private readonly UnixSocketStreamPiperInfo _info;
    public UnixSocketStreamPiperInfo Info =>_info;

    public UnixSocketStreamPiperFactory(UnixSocketStreamPiperInfo info)
    {
        _info = info;
    }

    public IPiper NewPiper()
    {
        return new UnixSocketStreamPiper(_info.EndPoint);
    }

    public static UnixSocketStreamPiperFactory TryParse(AddressElement element)
    {
        UnixSocketStreamPiperInfo info;

        if ((info = UnixSocketStreamPiperInfo.TryParse(element)) is not null)
            return new UnixSocketStreamPiperFactory(info);
        
        return null!;
    }
}