using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.RegularExpressions.Transformer.CSharpToPython
{
    public class CSharpToPythonTransformer : TextTransformer
    {
        // TODO:
        // ${...} -> \g<...>
        // \k<...> -> (?P=...)
        // (?<...> -> (?P<...>
        // null -> None
        // " + Environment.NewLine + " -> \n

        public static readonly IList<ISubstitutionRule> FirstStage = new List<SubstitutionRule>
        {
            // // 
            // # 
            (new Regex(@"//(\s*)(\r?\n)"), "#$1$2", 0),
            // // ...
            // # ...
            (new Regex(@"(?<before>(?<=\r?\n)(@""((?!"""")[^\n])*(""""[^\n]*)*""|[^""\n/])+)//(?<after>[^\n]+)(?<newline>\r?\n)"), "${before}#${after}${newline}", 0),
            // @" ... "" ... "
            // r" ... \" ... "
            (new Regex(@"(?<before>@""[^""]+)""""(?<after>(""""|[^""])+"")"), "${before}\\\"${after}", 1000),
            // @"
            // r"
            (new Regex(@"@"""), "r\"", 0),
            // new Regex(r"
            // r"
            (new Regex(@"new Regex\(r""(?<expression>((?!""\),)[^\n])+)""\),"), "r\"${expression}\",", 0),
            // r"{\s+[\r\n]+", "{\n"
            // r"{\s+[\r\n]+", r"{\n"
            (new Regex(@"(?<before>r""((?!"",)[^\n])+"",\s*)""(?<after>(\\""|[^""\n])*"")"), "${before}r\"${after}", 0),
            // r"$1"
            // r"\1"
            (new Regex(@"(?<before>r""(\\""|\$\D+|[^""\$\n])*)\$(?<number>\d+)(?<after>(\\""|[^""\n])*"")"), "${before}\\${number}${after}", 100),
            // "{" + Environment.NewLine,
            // "{\n",
            (new Regex(@"""((\\""|[^""\n])+)""\s*\+\s*Environment\.NewLine\s*,"), "\"$1\\n\",", 0),

        }.Cast<ISubstitutionRule>().ToList();

        public static readonly IList<ISubstitutionRule> LastStage = new List<SubstitutionRule>
        {

        }.Cast<ISubstitutionRule>().ToList();

        public CSharpToPythonTransformer(IList<ISubstitutionRule> extraRules) : base(FirstStage.Concat(extraRules).Concat(LastStage).ToList()) { }

        public CSharpToPythonTransformer() : base(FirstStage.Concat(LastStage).ToList()) { }
    }
}