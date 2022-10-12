namespace Firejox.App.WinSocat;

public class StdPiperInfo
{
    public static bool Check(AddressElement element)
    {
        return element.Tag.Equals("STDIO", StringComparison.OrdinalIgnoreCase);
    }
}

public class StdPiperFactory : IPiperFactory
{
    public IPiper NewPiper()
    {
        return new PairedStreamPiper(Console.OpenStandardInput(), Console.OpenStandardOutput());
    }

    public static StdPiperFactory TryParse(AddressElement element)
    {
        if (StdPiperInfo.Check(element))
            return new StdPiperFactory();
        return null!;
    }
}

public class StdPiperStrategy : PiperStrategy
{
    protected override IPiper NewPiper()
    {
        return new PairedStreamPiper(Console.OpenStandardInput(), Console.OpenStandardOutput());
    }

    public static StdPiperStrategy TryParse(AddressElement element)
    {
        if (StdPiperInfo.Check(element))
            return new StdPiperStrategy();
        return null!;
    }
}