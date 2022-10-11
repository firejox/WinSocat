using System.IO.Pipelines;

namespace APP;

public interface IPiper : IDisposable
{
    public PipeReader GetReader();
    public PipeWriter GetWriter();
}

public static class PiperExtensions
{
    public static async Task PipeBetween(this IPiper srcPiper, IPiper dstPiper)
    {
        var t1 = Task.Run(() => srcPiper.GetReader().CopyToAsync(dstPiper.GetWriter()));
        var t2 = Task.Run(() => dstPiper.GetReader().CopyToAsync(srcPiper.GetWriter()));
        await Task.WhenAny(t1, t2);
    }
}

public class Piper : IPiper
{
    private readonly PipeReader _reader;
    public PipeReader GetReader() => _reader;
    
    private readonly PipeWriter _writer;
    public PipeWriter GetWriter() => _writer;

    public Piper(PipeReader reader, PipeWriter writer)
    {
        _reader = reader;
        _writer = writer;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    { }
}

public class StreamPiper : Piper
{
    private Stream _stream;

    public Stream BaseStream => _stream;

    public StreamPiper(Stream stream) : base(PipeReader.Create(stream), PipeWriter.Create(stream))
    {
        _stream = stream;
    }

    protected override void Dispose(bool disposing)
    {
        try
        {
            if (disposing && _stream != null)
            {
                _stream.Close();
                _stream.Dispose();
            }
        }
        finally
        {
            _stream = null!;
        }
    }
}

public class PairedStreamPiper : Piper
{
    private Stream _readStream;
    public Stream ReadStream => _readStream;
    
    private Stream _writeStream;
    public Stream WriteStream => _writeStream;

    public PairedStreamPiper(Stream readStream, Stream writeStream) : base(PipeReader.Create(readStream),
        PipeWriter.Create(writeStream))
    {
        _readStream = readStream;
        _writeStream = writeStream;
    }

    protected override void Dispose(bool disposing)
    {
        try
        {
            if (disposing)
            {
                _readStream?.Dispose();
                _writeStream?.Dispose();
            }
        }
        finally
        {
            _readStream = null!;
            _writeStream = null!;
        }
    }
}

public interface IListenPiper : IDisposable
{
    public IPiper NewIncomingPiper();
    public Task<IPiper> NewIncomingPiperAsync();
    public void Close();
}