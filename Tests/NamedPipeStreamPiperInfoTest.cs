using APP;

namespace APPTest;

public class NamedPipeStreamPiperInfoTest
{ 
   [TestCase("NPIPE:fooServer:barPipe")]
   [TestCase("NPIPE::barPipe")]
   [TestCase("NPIPE:barPipe")]
   public void ValidInputParseTest(string input)
   {
      var element = AddressElement.TryParse(input);
      Assert.NotNull(APP.NamedPipeStreamPiperInfo.TryParse(element));
   }

   [TestCase("NPIPE:fooServer:barPipe")]
   [TestCase("npipe:fooServer:barPipe")]
   public void CaseInsensitiveValidInputParseTest(string input)
   {
      var element = AddressElement.TryParse(input);
      Assert.NotNull(APP.NamedPipeStreamPiperInfo.TryParse(element));
   }

   [TestCase("STDIO")]
   [TestCase("TCP:127.0.0.1:80")]
   [TestCase("TCP-LISTEN:127.0.0.1:80")]
   [TestCase("NPIPE-LISTEN:fooPipe")] 
   [TestCase(@"EXEC:'C:\Foo.exe bar'")]
   public void InvalidInputParseTest(string input)
   {
      var element = AddressElement.TryParse(input);
      Assert.Null(APP.NamedPipeStreamPiperInfo.TryParse(element));
   }

   [TestCase("NPIPE:fooServer:barPipe", ExpectedResult = "fooServer")]
   [TestCase("NPIPE::fooPipe", ExpectedResult = ".")]
   [TestCase("NPIPE:fooPipe", ExpectedResult = ".")]
   public string ServerPatternMatchTest(string input)
   {
      var element = AddressElement.TryParse(input);
      return NamedPipeStreamPiperInfo.TryParse(element).ServerName;
   }

   [TestCase("NPIPE:fooServer:barPipe", ExpectedResult = "barPipe")]
   [TestCase("NPIPE::fooPipe", ExpectedResult = "fooPipe")]
   [TestCase("NPIPE:fooPipe", ExpectedResult = "fooPipe")]
   public string PipePatternMatchTest(string input)
   {
      var element = AddressElement.TryParse(input);
      return NamedPipeStreamPiperInfo.TryParse(element).PipeName;
   }
}