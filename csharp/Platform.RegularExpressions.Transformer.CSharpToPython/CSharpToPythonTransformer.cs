using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.RegularExpressions.Transformer.CSharpToPython
{
    public class CSharpToPythonTransformer : TextTransformer
    {
        public static readonly IList<ISubstitutionRule> FirstStage = new List<SubstitutionRule>
        {
            // // 
            // # 
            (new Regex(@"//(\s*)(\r?\n)"), "#$1$2", 0),
            // // ...
            // # ...
            (new Regex(@"(?<before>(?<=\r?\n)(@""((?!"""")[^\n])*(""""[^\n]*)*""|[^""\n/])+)//(?<after>[^\n]+)(?<newline>\r?\n)"), "${before}#${after}${newline}", 0),
            // @" ... "" ... "
            // @" ... \" ... "
            (new Regex(@"(?<before>@""(\\""|[^""])*)""""(?<after>(\\""|[^""]|"""")*""(,|\)))"), "${before}\\\"${after}", 100),
            // "{" + Environment.NewLine,
            // "{\n",
            (new Regex(@"""((\\""|[^""\n])+)""\s*\+\s*Environment\.NewLine\s*,"), "\"$1\\n\",", 0),
            // " + Environment.NewLine + "
            // \n
            (new Regex(@"""\s*\+\s*Environment\.NewLine\s*\+\s*"""), "\\n", 0),
            // " + Environment.NewLine + Environment.NewLine + "
            // \n\n
            (new Regex(@"""\s*\+\s*Environment\.NewLine\s*\+\s*Environment\.NewLine\s*\+\s*"""), "\\n\\n", 0),
            // Environment.NewLine + Environment.NewLine
            // "\n\n"
            (new Regex(@"Environment\.NewLine\s*\+\s*Environment\.NewLine"), "\"\\n\\n\"", 0),
            // Environment.NewLine + "
            // "\n
            (new Regex(@"Environment\.NewLine\s*\+\s*"""), "\"\\n", 0),
            // @"
            // r"
            (new Regex(@"@"""), "r\"", 0),
            // new Regex(r"..."),
            // r"...",
            (new Regex(@"new Regex\(r""(?<expression>((?!""\),)[^\n])+)""\),"), "r\"${expression}\",", 0),
            // https://regex101.com/r/fM2da4/1/
            // ((?<parenthesis>\()|(?<-parenthesis>\))|[^();\r\n]*?)*?
            // (([^();\r\n]*?(?P<parenthesis>\()(?(parenthesis)[^();\r\n]*?\)))*?[^();\r\n]*?)
            (new Regex(@"\((?<expressionName>\?<\w+>)?\((\?<(?<symbolName>\w+)>)(?<openSymbol>(\(|[^()|])+)\)\|\((\?<-\k<symbolName>>)(?<closeSymbol>(\)|[^()|])+)\)\|(?<otherSymbols>(\[[^\[\]]+\]|[^()|])+(\|(\[[^\[\]]+]|[^()|])+)?)\)(?<multiplicator>(\*|\+)(\?)?)"), "(${expressionName}((${otherSymbols})(?P<${symbolName}>${openSymbol})(?(${symbolName})(${otherSymbols})${closeSymbol}))${multiplicator}(${otherSymbols}))", 0),
            // (?<before>
            // (?P<before>
            (new Regex(@"(?<before>\(\?)(?<after><(?!!)[^<>]+>)"), "${before}P${after}", 0),
            // \k<...>
            // (?P=...)
            (new Regex(@"\\k<(?<name>[^<>]+)>"), "(?P=${name})", 0),
            // r"{\s+[\r\n]+", "{\n"
            // r"{\s+[\r\n]+", r"{\n"
            (new Regex(@"(?<before>r""((?!"",)[^\n])+"",\s*)""(?<after>(\\""|[^""\n])*"")"), "${before}r\"${after}", 0),
            // r"${1}"
            // r"\g<1>"
            (new Regex(@"(?<before>r""(\\""|\$\D+|[^""\$\n])*)\${(?<name>\w+)}(?<after>(\\""|[^""\n])*"")"), "${before}\\g<${name}>${after}", 100),
            // r"$1"
            // r"\1"
            (new Regex(@"(?<before>r""(\\""|\$\D+|[^""\$\n])*)\$(?<number>\d+)(?<after>(\\""|[^""\n])*"")"), "${before}\\${number}${after}", 100),
        }.Cast<ISubstitutionRule>().ToList();

        public static readonly IList<ISubstitutionRule> LastStage = new List<SubstitutionRule>
        {

        }.Cast<ISubstitutionRule>().ToList();

        public CSharpToPythonTransformer(IList<ISubstitutionRule> extraRules) : base(FirstStage.Concat(extraRules).Concat(LastStage).ToList()) { }

        public CSharpToPythonTransformer() : base(FirstStage.Concat(LastStage).ToList()) { }
    }
}