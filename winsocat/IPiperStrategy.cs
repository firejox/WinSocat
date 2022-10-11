namespace APP;

public interface IPiperStrategy
{
    public void Execute(IPiperFactory factory);
    public Task ExecuteAsync(IPiperFactory factory, CancellationToken token = default);
}


public abstract class PiperStrategy : IPiperStrategy
{
    protected abstract IPiper NewPiper();

    public virtual void Execute(IPiperFactory factory)
    {
        using (var srcPiper = NewPiper())
        {
            using (var dstPiper = factory.NewPiper())
            {
                srcPiper.PipeBetween(dstPiper).Wait();
            }
        }
    }

    public virtual async Task ExecuteAsync(IPiperFactory factory, CancellationToken token = default)
    {
        using (var srcPiper = NewPiper())
        {
            using (var dstPiper = factory.NewPiper())
            {
                await srcPiper.PipeBetween(dstPiper);
            }
        }
    }
}

public abstract class ListenPiperStrategy : IPiperStrategy
{
    protected abstract IListenPiper NewListenPiper();

    public virtual void Execute(IPiperFactory factory)
    {
        using (var listenPiper = NewListenPiper())
        {
            while (true)
            {
                var srcPiper = listenPiper.NewIncomingPiper();
                _ = Task.Run(async () =>
                {
                    using (srcPiper)
                    {
                        using (var dstPiper = factory.NewPiper())
                        {
                            await srcPiper.PipeBetween(dstPiper);
                        }
                    }
                });
            }
        }
    }
    
    public virtual async Task ExecuteAsync(IPiperFactory factory, CancellationToken token = default)
    {
        using (var listenPiper = NewListenPiper())
        {
            token.Register(() => listenPiper.Close());
            while (true)
            {
                var srcPiper = await listenPiper.NewIncomingPiperAsync();
                _ = Task.Run(async () =>
                {
                    using (srcPiper)
                    {
                        using (var dstPiper = factory.NewPiper())
                        {
                            await srcPiper.PipeBetween(dstPiper);
                        }
                    }
                });
            }
        }
        
    }
}
