﻿using System.Diagnostics;
using System.IO.Pipelines;
using System.CommandLine.Parsing;

namespace Firejox.App.WinSocat;

public class ProcPiperInfo
{
    private string _filename;
    private string _arguments;

    public string FileName => _filename;
    public string Arguments => _arguments;

    public ProcPiperInfo(string filename, string arguments)
    {
        _filename = filename;
        _arguments = arguments;
    }

    public static ProcPiperInfo TryParse(AddressElement element)
    {
        if (!element.Tag.Equals("EXEC", StringComparison.OrdinalIgnoreCase)) return null!;
        var execPattern = element.Address;
        var cmdLine = CommandLineStringSplitter.Instance.Split(execPattern);
        string filename = cmdLine.First();
        string arguments = String.Join(' ', cmdLine.Skip(1));

        return new ProcPiperInfo(filename, arguments);
    }

}

public class ProcPiper : IPiper
{
    private Process _process;
    private PipeReader _reader;
    private PipeWriter _writer;

    public ProcPiper(string filename, string arguments)
    {
        _process = new Process();
        _process.StartInfo.FileName = filename;
        _process.StartInfo.Arguments = arguments;
        _process.StartInfo.UseShellExecute = false;
        _process.StartInfo.CreateNoWindow = true;
        _process.StartInfo.RedirectStandardInput = true;
        _process.StartInfo.RedirectStandardOutput = true;
        _process.Start();
        
        _reader = PipeReader.Create(_process.StandardOutput.BaseStream);
        _writer = PipeWriter.Create(_process.StandardInput.BaseStream);
    }

    public PipeReader GetReader() => _reader;
    public PipeWriter GetWriter() => _writer;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        try
        {
            if (disposing && _process != null)
            {
                _process.Kill();
                _process.Dispose();
            }
        }
        finally
        {
            _process = null!;
        }
    }
}

public class ProcPiperFactory : IPiperFactory
{
    private readonly ProcPiperInfo _info;

    public ProcPiperInfo Info => _info;
    public ProcPiperFactory(ProcPiperInfo info)
    {
        _info = info;
    }

    public IPiper NewPiper()
    {
        return new ProcPiper(_info.FileName, _info.Arguments);
    }

    public static ProcPiperFactory TryParse(AddressElement element)
    {
        ProcPiperInfo info;

        if ((info = ProcPiperInfo.TryParse(element)) != null)
            return new ProcPiperFactory(info);

        return null!;
    }
}

public class ProcPiperStrategy : PiperStrategy
{
    private readonly ProcPiperInfo _info;

    public ProcPiperInfo Info => _info;

    public ProcPiperStrategy(ProcPiperInfo info)
    {
        _info = info;
    }

    protected override IPiper NewPiper()
    {
        return new ProcPiper(_info.FileName, _info.Arguments);
    }

    public static ProcPiperStrategy TryParse(AddressElement element)
    {
        ProcPiperInfo info;

        if ((info = ProcPiperInfo.TryParse(element)) != null)
            return new ProcPiperStrategy(info);

        return null!;
    }
}