
using System.Net;

namespace APP;

public class WslPiperInfo
{
    private static bool WslCheck = File.Exists(@"C:\Windows\System32\wsl.exe");
    private string _command;
    public string Command => _command;

    private string _distribution;
    public string Distribution => _distribution;

    private string _user;
    public string User => _user;

    public WslPiperInfo(string command, string distribution, string user)
    {
        _command = command;
        _distribution = distribution;
        _user = user;
    }

    public ProcPiperInfo ToProcPiperInfo()
    {
        string arguments = "";
        
        if (!_distribution.Equals(""))
        {
            arguments += "-d " + _distribution + " ";
        }

        if (!_user.Equals(""))
        {
            arguments += "-u " + _user + " ";
        }

        arguments += _command;

        return new ProcPiperInfo(@" C:\Windows\System32\wsl.exe", arguments);
    }

    public static WslPiperInfo TryParse(AddressElement element)
    {
        if (!WslCheck)
            return null!;
        if (!element.Tag.Equals("WSL", StringComparison.OrdinalIgnoreCase))
            return null!;

        string command = element.Address.Trim('\"', '\'');
        string distribution;
        string user;

        if (!element.Options.TryGetValue("distribution", out distribution))
            distribution = "";

        if (!element.Options.TryGetValue("user", out user))
            user = "";

        return new WslPiperInfo(command, distribution, user);
    }
}

public class WslPiperFactory
{
    public static ProcPiperFactory TryParse(AddressElement element)
    {
        WslPiperInfo info;

        if ((info = WslPiperInfo.TryParse(element)) != null)
            return new ProcPiperFactory(info.ToProcPiperInfo());
        return null!;
    }
}

public class WslPiperStrategy
{
    public static ProcPiperStrategy TryParse(AddressElement element)
    {
        WslPiperInfo info;

        if ((info = WslPiperInfo.TryParse(element)) != null)
            return new ProcPiperStrategy(info.ToProcPiperInfo());
        return null!;
    }
}
