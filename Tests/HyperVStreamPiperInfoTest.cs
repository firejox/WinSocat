using Firejox.App.WinSocat;
namespace APPTest;

public class HyperVStreamPiperInfoTest
{
    public const string GuidZero      = "00000000-0000-0000-0000-000000000000";
    public const string GuidWildCard  = "00000000-0000-0000-0000-000000000000";
    public const string GuidBroadCast = "ffffffff-ffff-ffff-ffff-ffffffffffff";
    public const string GuidChildren  = "90db8b89-0d35-4f79-8ce9-49ea0ac8b7cd";
    public const string GuidLoopBack  = "e0e16197-dd56-4a10-9195-5ee7a155a838";
    public const string GuidParent    = "a42e7cda-d03f-480c-9cc2-a4de20abb878";
    public const string GuidVSock0    = "00000000-facb-11e6-bd58-64006a7986d3";
    public const string GuidVSock1    = "00000001-facb-11e6-bd58-64006a7986d3";
    public const string GuidVSockMax  = "7fffffff-facb-11e6-bd58-64006a7986d3";

    [TestCase("HVSOCK:00000000-0000-0000-0000-000000000000:00000000-0000-0000-0000-000000000000")]
    [TestCase("HVSOCK:e0e16197dd564a1091955ee7a155a838:(00000000-0000-0000-0000-000000000000)")]
    [TestCase("HVSOCK:ZERO:VSOCK-0")]
    [TestCase("HVSOCK:PARENT:VSOCK-0")]
    [TestCase("HVSOCK:WILDCARD:VSOCK-0")]
    [TestCase("HVSOCK:LOOPBACK:VSOCK-0")]
    [TestCase("HVSOCK:BROADCAST:VSOCK-0")]
    [TestCase("HVSOCK:CHILDREN:VSOCK-0")]
    public void ValidInputParseTest(string input)
    {
        var element = AddressElement.TryParse(input);
        Assert.NotNull(HyperVStreamPiperInfo.TryParse(element));
    }
    
    [TestCase("HVSOCK:00000000-0000-0000-0000-000000000000:00000000-0000-0000-0000-000000000000")]
    [TestCase("hvsock:e0e16197dd564a1091955ee7a155a838:(00000000-0000-0000-0000-000000000000)")]
    [TestCase("HVSOCK:zero:vsock-0")]
    [TestCase("HVSOCK:parent:vsock-0")]
    [TestCase("HVSOCK:wildcard:vsock-0")]
    [TestCase("HVSOCK:loopback:vsock-0")]
    [TestCase("HVSOCK:broadcast:vsock-0")]
    [TestCase("HVSOCK:children:vsock-0")]
    public void CaseInsensitiveValidInputParseTest(string input)
    {
        var element = AddressElement.TryParse(input);
        Assert.NotNull(HyperVStreamPiperInfo.TryParse(element));
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
        Assert.Null(HyperVStreamPiperInfo.TryParse(element));
    }

    [TestCase("HVSOCK:00000000-0000-0000-0000-000000000000:00000000-0000-0000-0000-000000000000", ExpectedResult = "00000000-0000-0000-0000-000000000000")]
    [TestCase("HVSOCK:e0e16197dd564a1091955ee7a155a838:(00000000-0000-0000-0000-000000000000)", ExpectedResult = "e0e16197-dd56-4a10-9195-5ee7a155a838")]
    [TestCase("HVSOCK:ZERO:VSOCK-0", ExpectedResult = GuidZero)]
    [TestCase("HVSOCK:WILDCARD:VSOCK-0", ExpectedResult = GuidWildCard)]
    [TestCase("HVSOCK:BROADCAST:VSOCK-0", ExpectedResult = GuidBroadCast)]
    [TestCase("HVSOCK:CHILDREN:VSOCK-0", ExpectedResult = GuidChildren)]
    [TestCase("HVSOCK:PARENT:VSOCK-0", ExpectedResult = GuidParent)]
    [TestCase("HVSOCK:LOOPBACK:VSOCK-0", ExpectedResult = GuidLoopBack)]
    public string VmIdPatternMatchTest(string input)
    {
        var element = AddressElement.TryParse(input);
        return HyperVStreamPiperInfo.TryParse(element).VmId.ToString();
    }

    [TestCase("HVSOCK:00000000-0000-0000-0000-000000000000:00000000-0000-0000-0000-000000000000", ExpectedResult = "00000000-0000-0000-0000-000000000000")]
    [TestCase("HVSOCK:90db8b89-0d35-4f79-8ce9-49ea0ac8b7cd:e0e16197dd564a1091955ee7a155a838", ExpectedResult = "e0e16197-dd56-4a10-9195-5ee7a155a838")]
    [TestCase("HVSOCK:ZERO:VSOCK-0", ExpectedResult = GuidVSock0)]
    [TestCase("HVSOCK:ZERO:VSOCK-1", ExpectedResult = GuidVSock1)]
    [TestCase("HVSOCK:ZERO:VSOCK-2147483647", ExpectedResult = GuidVSockMax)]
    public string ServiceIdPatternMatchTest(string input)
    {
        var element = AddressElement.TryParse(input);
        return HyperVStreamPiperInfo.TryParse(element).ServiceId.ToString();
    }
}