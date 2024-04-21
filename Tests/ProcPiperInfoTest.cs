﻿using Firejox.App.WinSocat;

namespace APPTest;

public class ProcPiperInfoTest
{
    [TestCase(@"EXEC:C:\Foo.exe")]
    [TestCase(@"EXEC:C:\Foo.exe bar")]
    [TestCase(@"EXEC:'C:\Foo.exe bar'")]
    public void ValidInputParseTest(string input)
    {
        var element = AddressElement.TryParse(input);
        Assert.NotNull(Firejox.App.WinSocat.ProcPiperInfo.TryParse(element));
    }

    [TestCase(@"EXEC:C:\Foo.exe bar")]
    [TestCase(@"exec:C:\Foo.exe bar")]
    public void CaseInsensitiveValidInputParseTest(string input)
    {
        var element = AddressElement.TryParse(input);
        Assert.NotNull(Firejox.App.WinSocat.ProcPiperInfo.TryParse(element));
    }

    [TestCase("STDIO")]
    [TestCase("TCP:127.0.0.1:80")]
    [TestCase("TCP-LISTEN:127.0.0.1:80")]
    [TestCase("NPIPE:fooServer:barPipe")]
    [TestCase("NPIPE-LISTEN:fooPipe")]
    public void InvalidInputParseTest(string input)
    {
        var element = AddressElement.TryParse(input);
        Assert.Null(Firejox.App.WinSocat.ProcPiperInfo.TryParse(element));
    }

    [TestCase(@"EXEC:C:\Foo.exe bar", ExpectedResult = @"C:\Foo.exe")]
    [TestCase(@"EXEC:C:\Foo.exe", ExpectedResult = @"C:\Foo.exe")]
    [TestCase(@"EXEC:""C:\foo\space dir\bar.exe"" arg1 arg2", ExpectedResult = @"C:\foo\space dir\bar.exe")]
    public string FileNamePatternMatchTest(string input)
    {
        var element = AddressElement.TryParse(input);
        return Firejox.App.WinSocat.ProcPiperInfo.TryParse(element).FileName;
    }

    [TestCase(@"EXEC:C:\Foo.exe bar1 bar2", ExpectedResult = "bar1 bar2")]
    [TestCase(@"EXEC:""C:\foo\space dir\bar.exe"" arg1 arg2", ExpectedResult = @"arg1 arg2")]
    public string ArgumentPatternMatchTest(string input)
    {
        var element = AddressElement.TryParse(input);
        return Firejox.App.WinSocat.ProcPiperInfo.TryParse(element).Arguments;
    }
}