using Xunit;

namespace Platform.RegularExpressions.Transformer.CSharpToPython.Tests
{
    public class CSharpToCppTransformerTests
    {
        [Fact]
        public void EmptyLineTest()
        {
            // This test can help to test basic problems with regular expressions like incorrect syntax
            var transformer = new CSharpToPythonTransformer();
            var actualResult = transformer.Transform("");
            Assert.Equal("", actualResult);
        }

        [Fact]
        public void RulesTranslation()
        {
            // Based on https://github.com/linksplatform/RegularExpressions.Transformer.CSharpToCpp/blob/4cb96825f9271d30c05cb4dabead246cb79d856d/csharp/Platform.RegularExpressions.Transformer.CSharpToCpp/CSharpToCppTransformer.cs
            var source = @"
            // // ...
            // 
            (new Regex(@""(\r ?\n)?[ \t] +//+.+""), """", 0),
            // #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
            // 
            (new Regex(@""^\s*?\#pragma[\sa-zA-Z0-9]+$""), """", 0),
            // {\n\n\n
            // {
            (new Regex(@""{\s+[\r\n]+""), ""{"" + Environment.NewLine, 0),
            // Platform.Collections.Methods.Lists
            // Platform::Collections::Methods::Lists
            (new Regex(@""(namespace[^\r\n]+?)\.([^\r\n]+?)""), ""$1::$2"", 20),
";

            // Based on https://github.com/linksplatform/RegularExpressions.Transformer.CSharpToCpp/blob/4cb96825f9271d30c05cb4dabead246cb79d856d/python/cs2cpp/CSharpToCpp.py
            var target = @"
            # // ...
            # 
            (r""(\r ?\n)?[ \t] +//+.+"", r"""", 0),
            # #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
            # 
            (r""^\s*?\#pragma[\sa-zA-Z0-9]+$"", r"""", 0),
            # {\n\n\n
            # {
            (r""{\s+[\r\n]+"", r""{\n"", 0),
            # Platform.Collections.Methods.Lists
            # Platform::Collections::Methods::Lists
            (r""(namespace[^\r\n]+?)\.([^\r\n]+?)"", r""\1::\2"", 20),
";
            
            var transformer = new CSharpToPythonTransformer();
            var actualResult = transformer.Transform(source);
            Assert.Equal(target, actualResult);
        }

        [Fact]
        public void HelloWorldTest()
        {

        }
    }
}