using System.CommandLine;

namespace APP
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var arg1 = new Argument<string>("address1");
            var arg2 = new Argument<string>("address2");

            var rootCommand = new RootCommand();
            rootCommand.Add(arg1);
            rootCommand.Add(arg2);
            
            rootCommand.SetHandler(async (string address1, string address2) =>
            {
                var strategy = PiperStrategyParse(address1);
                
                if (strategy == null)
                {
                    Console.Error.WriteLine("\"{0}\" is not available on [address1]", address1);
                    return;
                }

                var factory = PiperFactoryParse(address2);
                if (factory == null)
                {
                    Console.Error.WriteLine("\"{0}\" is not available on [address2]", address2);
                    return;
                }

                await strategy.ExecuteAsync(factory);
            }, arg1, arg2);

            await rootCommand.InvokeAsync(args);
        }

        public static IPiperStrategy PiperStrategyParse(string input)
        {
            AddressElement element = AddressElement.TryParse(input);

            if (element == null)
                return null!;
            
            IPiperStrategy strategy;

            if ((strategy = StdPiperStrategy.TryParse(element)) != null)
                return strategy;
            
            if ((strategy = TcpStreamPiperStrategy.TryParse(element)) != null)
                return strategy;
            if ((strategy = TcpListenPiperStrategy.TryParse(element)) != null)
                return strategy;
            
            if ((strategy = ProcPiperStrategy.TryParse(element)) != null)
                return strategy;

            if ((strategy = NamedPipeStreamPiperStrategy.TryParse(element)) != null)
                return strategy;
            if ((strategy = NamedPipeListenPiperStrategy.TryParse(element)) != null)
                return strategy;

            if ((strategy = WslPiperStrategy.TryParse(element)) != null)
                return strategy;

            return null!;
        }

        public static PiperFactory PiperFactoryParse(string input)
        {
            var element = AddressElement.TryParse(input);

            if (element == null)
                return null!;
            
            PiperFactory factory;

            if ((factory = StdPiperFactory.TryParse(element)) != null)
                return factory;

            if ((factory = TcpStreamPiperFactory.TryParse(element)) != null)
                return factory;

            if ((factory = ProcPiperFactory.TryParse(element)) != null)
                return factory;

            if ((factory = NamedPipeStreamPiperFactory.TryParse(element)) != null)
                return factory;

            if ((factory = WslPiperFactory.TryParse(element)) != null)
                return factory;
            
            return null!;
        }

    }
}