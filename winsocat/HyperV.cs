using System.Net.Sockets;
using System.Net;

namespace Firejox.App.WinSocat;
internal static class HyperV
{
    public const AddressFamily AddressFamily = (AddressFamily)34;
    public const ProtocolType ProtocolType = (ProtocolType)1;
    private const int SocketAddressSize = 36;

    private static readonly Guid GuidZero = Guid.Empty;
    private static readonly Guid GuidWildCard = Guid.Empty;
    private static readonly Guid GuidBroadCast = new Guid("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF");
    private static readonly Guid GuidChildren = new Guid("90db8b89-0d35-4f79-8ce9-49ea0ac8b7cd");
    private static readonly Guid GuidLoopBack = new Guid("e0e16197-dd56-4a10-9195-5ee7a155a838");
    private static readonly Guid GuidParent = new Guid("a42e7cda-d03f-480c-9cc2-a4de20abb878");
    
    public class EndPoint : System.Net.EndPoint
    {
        private Guid _vmId;
        private Guid _serviceId;
        
        public EndPoint(Guid vmId, Guid serviceId)
        {
            _vmId = vmId;
            _serviceId = serviceId;
        }

        public override AddressFamily AddressFamily => HyperV.AddressFamily;

        public Guid VmId
        {
            get { return _vmId;  }
            set { _vmId = value; }
        }

        public Guid ServiceId
        {
            get { return _serviceId; }
            set { _serviceId = value; }
        }
        
        public override System.Net.EndPoint Create(SocketAddress addr)
        {
            if (addr is null || 
                addr.Family != AddressFamily || 
                addr.Size != SocketAddressSize)
            {
                return null!;
            }

            byte[] vmIdArr = new byte[16];
            byte[] serviceIdArr = new byte[16];

            for (int i = 0; i < vmIdArr.Length; i++)
                vmIdArr[i] = addr[i + 4];

            for (int i = 0; i < serviceIdArr.Length; i++)
                serviceIdArr[i] = addr[i + 20];
            
            var vmId = new Guid(vmIdArr);
            var serviceId = new Guid(serviceIdArr);
            
            return new EndPoint(
                vmId,
                serviceId
            );
        }
        
        public override SocketAddress Serialize()
        {
            SocketAddress addr = new SocketAddress(AddressFamily, SocketAddressSize);
            byte[] vmRaw = _vmId.ToByteArray();
            byte[] serviceRaw = _serviceId.ToByteArray();

            addr[2] = 0;

            for (int i = 0; i < vmRaw.Length; i++) 
                addr[i + 4] = vmRaw[i];

            for (int i = 0; i < serviceRaw.Length; i++) 
                addr[i + 20] = serviceRaw[i];

            return addr;
        }

        public override string ToString()
        {
            return _vmId.ToString() + _serviceId.ToString();
        }

        public override bool Equals(object? obj)
        {
            EndPoint? endPoint = (EndPoint?)obj;
            return endPoint is not null && 
                   endPoint._vmId == _vmId && 
                   endPoint._serviceId == _serviceId;
        }

        public override int GetHashCode()
        {
            return Serialize().GetHashCode();
        }
    }

    public static Guid GetVmGuid(string addr)
    {
        if (addr.Equals("ZERO", StringComparison.OrdinalIgnoreCase))
            return GuidZero;
        if (addr.Equals("WILDCARD", StringComparison.OrdinalIgnoreCase))
            return GuidWildCard;
        if (addr.Equals("BROADCAST", StringComparison.OrdinalIgnoreCase))
            return GuidBroadCast;
        if (addr.Equals("LOOPBACK", StringComparison.OrdinalIgnoreCase))
            return GuidLoopBack;
        if (addr.Equals("CHILDREN", StringComparison.OrdinalIgnoreCase))
            return GuidChildren;
        if (addr.Equals("PARENT", StringComparison.OrdinalIgnoreCase))
            return GuidParent;
        return new Guid(addr);
    }

    public static Guid GetServiceId(string addr)
    {
        if (addr.StartsWith("VSOCK-", StringComparison.OrdinalIgnoreCase))
        {
            uint port = UInt32.Parse(addr.Substring(6));
            return new Guid(port, 0xfacb, 0x11e6, 0xbd, 0x58, 0x64, 0x00, 0x6a, 0x79, 0x86, 0xd3);
        }

        return new Guid(addr);
    }
}

public class HyperVStreamPiperInfo
{
    private readonly HyperV.EndPoint _endPoint;
    internal HyperV.EndPoint EndPoint => _endPoint;

    public Guid VmId => _endPoint.VmId;
    public Guid ServiceId => _endPoint.ServiceId;

    public HyperVStreamPiperInfo(Guid vmId, Guid serviceId)
    {
        _endPoint = new HyperV.EndPoint(vmId, serviceId);
    }

    public static HyperVStreamPiperInfo TryParse(AddressElement element)
    {
        if (!element.Tag.Equals("HVSOCK", StringComparison.OrdinalIgnoreCase)) return null!;

        int sepIdx = element.Address.LastIndexOf(':');

        if (sepIdx == -1 || sepIdx == 0)
            return null!;

        Guid vmId = HyperV.GetVmGuid(element.Address.Substring(0, sepIdx));
        Guid serviceId = HyperV.GetServiceId(element.Address.Substring(sepIdx + 1));

        return new HyperVStreamPiperInfo(vmId, serviceId);
    }
}

public class HyperVListenPiperInfo
{
    private readonly HyperV.EndPoint _endPoint;
    internal HyperV.EndPoint EndPoint => _endPoint;

    public Guid VmId => _endPoint.VmId;
    public Guid ServiceId => _endPoint.ServiceId;

    public HyperVListenPiperInfo(Guid vmId, Guid serviceId)
    {
        _endPoint = new HyperV.EndPoint(vmId, serviceId);
    }

    public static HyperVListenPiperInfo TryParse(AddressElement element)
    {
        if (!element.Tag.Equals("HVSOCK-LISTEN", StringComparison.OrdinalIgnoreCase)) return null!;

        int sepIdx = element.Address.LastIndexOf(':');

        if (sepIdx == -1 || sepIdx == 0)
            return null!;

        Guid vmId = HyperV.GetVmGuid(element.Address.Substring(0, sepIdx));
        Guid serviceId = HyperV.GetServiceId(element.Address.Substring(sepIdx + 1));

        return new HyperVListenPiperInfo(vmId, serviceId);
    }
}

public class HyperVStreamPiper : StreamPiper
{
    private Socket _socket;

    public HyperVStreamPiper(Socket socket) : base(new NetworkStream(socket))
    {
        _socket = socket;
    }

    internal HyperVStreamPiper(HyperV.EndPoint endPoint) : this(ConnectSocket(endPoint)) 
    {
    }

    protected override void Dispose(bool disposing)
    {
        try
        {
            base.Dispose(disposing);
            if (disposing && _socket is not null) 
                _socket.Dispose();
        }
        finally
        {
            _socket = null!;
        }
    }

    private static Socket ConnectSocket(HyperV.EndPoint endPoint)
    {
        var socket = new Socket(HyperV.AddressFamily, SocketType.Stream, HyperV.ProtocolType);
        socket.Connect(endPoint);
        return socket;
    }
}

public class HyperVListenPiper : IListenPiper
{
    private Socket _socket;
    
    internal HyperVListenPiper(HyperV.EndPoint endPoint)
    {
        _socket = new Socket(HyperV.AddressFamily, SocketType.Stream, HyperV.ProtocolType);
        _socket.Bind(endPoint);
        _socket.Listen();
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
            if (disposing && _socket is not null) 
                _socket.Dispose();
        }
        finally
        {
            _socket = null!;
        }
    }

    public IPiper NewIncomingPiper()
    {
        return new HyperVStreamPiper(_socket.Accept());
    }

    public async Task<IPiper> NewIncomingPiperAsync()
    {
        var socket = await _socket.AcceptAsync();
        return new HyperVStreamPiper(socket);
    }

    public void Close()
    {
        _socket.Close();
    }
}

public class HyperVStreamPiperStrategy : PiperStrategy
{
    private readonly HyperVStreamPiperInfo _info;
    public HyperVStreamPiperInfo Info => _info;

    public HyperVStreamPiperStrategy(HyperVStreamPiperInfo info)
    {
        _info = info;
    }

    protected override IPiper NewPiper()
    {
        return new HyperVStreamPiper(_info.EndPoint);
    }

    public static HyperVStreamPiperStrategy TryParse(AddressElement element)
    {
        HyperVStreamPiperInfo info;
        if ((info = HyperVStreamPiperInfo.TryParse(element)) is not null)
            return new HyperVStreamPiperStrategy(info);
        return null!;
    }
}

public class HyperVListenPiperStrategy : ListenPiperStrategy
{
    private readonly HyperVListenPiperInfo _info;

    public HyperVListenPiperStrategy(HyperVListenPiperInfo info)
    {
        _info = info;
    }
    
    protected override IListenPiper NewListenPiper()
    {
        return new HyperVListenPiper(_info.EndPoint);
    }

    public static HyperVListenPiperStrategy TryParse(AddressElement element)
    {
        HyperVListenPiperInfo info;

        if ((info = HyperVListenPiperInfo.TryParse(element)) is not null)
            return new HyperVListenPiperStrategy(info);

        return null!;
    }
}

public class HyperVStreamPiperFactory : IPiperFactory
{
    private readonly HyperVStreamPiperInfo _info;
    public HyperVStreamPiperInfo Info => _info;

    public HyperVStreamPiperFactory(HyperVStreamPiperInfo info)
    {
        _info = info;
    }

    public IPiper NewPiper()
    {
        return new HyperVStreamPiper(_info.EndPoint);
    }

    public static HyperVStreamPiperFactory TryParse(AddressElement element)
    {
        HyperVStreamPiperInfo info;
        if ((info = HyperVStreamPiperInfo.TryParse(element)) is not null)
            return new HyperVStreamPiperFactory(info);

        return null!;
    }
}