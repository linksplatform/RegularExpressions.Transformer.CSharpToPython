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
            // out TProduct
            // TProduct
            (new Regex(@""(?<before>(<|, ))(in|out) (?<typeParameter>[a-zA-Z0-9]+)(?<after>(>|,))""), ""${before}${typeParameter}${after}"", 10),
            // public ...
            // public: ...
            (new Regex(@""(?<newLineAndIndent>\r?\n?[ \t]*)(?<before>[^\{\(\r\n]*)(?<access>private|protected|public)[ \t]+(?![^\{\(\r\n]*(interface|class|struct)[^\{\(\r\n]*[\{\(\r\n])""), ""${newLineAndIndent}${access}: ${before}"", 0),
            // public: static bool CollectExceptions { get; set; }
            // public: inline static bool CollectExceptions;
            (new Regex(@""(?<access>(private|protected|public): )(?<before>(static )?[^\r\n]+ )(?<name>[a-zA-Z0-9]+) {[^;}]*(?<=\W)get;[^;}]*(?<=\W)set;[^;}]*}""), ""${access}inline ${before}${name};"", 0),
            // public abstract class
            // class
            (new Regex(@""((public|protected|private|internal|abstract|static) )*(?<category>interface|class|struct)""), ""${category}"", 0),
            // class GenericCollectionMethodsBase<TElement> {
            // template <typename TElement> class GenericCollectionMethodsBase {
            (new Regex(@""class ([a-zA-Z0-9]+)<([a-zA-Z0-9]+)>([^{]+){""), ""template <typename $2> class $1$3{"", 0),
            // static void TestMultipleCreationsAndDeletions<TElement>(SizedBinaryTreeMethodsBase<TElement> tree, TElement* root)
            // template<typename T> static void TestMultipleCreationsAndDeletions<TElement>(SizedBinaryTreeMethodsBase<TElement> tree, TElement* root)
            (new Regex(@""static ([a-zA-Z0-9]+) ([a-zA-Z0-9]+)<([a-zA-Z0-9]+)>\(([^\)\r\n]+)\)""), ""template <typename $3> static $1 $2($4)"", 0),
            // interface IFactory<out TProduct> {
            // template <typename TProduct> class IFactory { public:
            (new Regex(@""interface (?<interface>[a-zA-Z0-9]+)<(?<typeParameters>[a-zA-Z0-9 ,]+)>(?<whitespace>[^{]+){""), ""template <typename...> class ${interface}; template <typename ${typeParameters}> class ${interface}<${typeParameters}>${whitespace}{"" + Environment.NewLine + ""    public:"", 0),
            // template <typename TObject, TProperty, TValue>
            // template <typename TObject, typename TProperty, TValue>
            (new Regex(@""(?<before>template <((, )?typename [a-zA-Z0-9]+)+, )(?<typeParameter>[a-zA-Z0-9]+)(?<after>(,|>))""), ""${before}typename ${typeParameter}${after}"", 10),
            // Insert markers
            // private: static void BuildExceptionString(this StringBuilder sb, Exception exception, int level)
            // /*~extensionMethod~BuildExceptionString~*/private: static void BuildExceptionString(this StringBuilder sb, Exception exception, int level)
            (new Regex(@""private: static [^\r\n]+ (?<name>[a-zA-Z0-9]+)\(this [^\)\r\n]+\)""), ""/*~extensionMethod~${name}~*/$0"", 0),
            // Move all markers to the beginning of the file.
            (new Regex(@""\A(?<before>[^\r\n]+\r?\n(.|\n)+)(?<marker>/\*~extensionMethod~(?<name>[a-zA-Z0-9]+)~\*/)""), ""${marker}${before}"", 10),
            // /*~extensionMethod~BuildExceptionString~*/...sb.BuildExceptionString(exception.InnerException, level + 1);
            // /*~extensionMethod~BuildExceptionString~*/...BuildExceptionString(sb, exception.InnerException, level + 1);
            (new Regex(@""(?<before>/\*~extensionMethod~(?<name>[a-zA-Z0-9]+)~\*/(.|\n)+\W)(?<variable>[_a-zA-Z0-9]+)\.\k<name>\(""), ""${before}${name}(${variable}, "", 50),
            // Remove markers
            // /*~extensionMethod~BuildExceptionString~*/
            // 
            (new Regex(@""/\*~extensionMethod~[a-zA-Z0-9]+~\*/""), """", 0),
            // (this 
            // (
            (new Regex(@""\(this ""), ""("", 0),
            // public: static readonly EnsureAlwaysExtensionRoot Always = new EnsureAlwaysExtensionRoot();
            // public:inline static EnsureAlwaysExtensionRoot Always;
            (new Regex(@""(?<access>(private|protected|public): )?static readonly (?<type>[a-zA-Z0-9]+) (?<name>[a-zA-Z0-9_]+) = new \k<type>\(\);""), ""${access}inline static ${type} ${name};"", 0),
            // public: static readonly string ExceptionContentsSeparator = ""---"";
            // public: inline static const char* ExceptionContentsSeparator = ""---"";
            (new Regex(@""(?<access>(private|protected|public): )?static readonly string (?<name>[a-zA-Z0-9_]+) = """"(?<string>(\""""|[^""""\r\n])+)"""";""), ""${access}inline static const char* ${name} = \""${string}\"";"", 0),
            // private: const int MaxPath = 92;
            // private: static const int MaxPath = 92;
            (new Regex(@""(?<access>(private|protected|public): )?(const|static readonly) (?<type>[a-zA-Z0-9]+) (?<name>[_a-zA-Z0-9]+) = (?<value>[^;\r\n]+);""), ""${access}static const ${type} ${name} = ${value};"", 0),
            //  ArgumentNotNull(EnsureAlwaysExtensionRoot root, TArgument argument) where TArgument : class
            //  ArgumentNotNull(EnsureAlwaysExtensionRoot root, TArgument* argument)
            (new Regex(@""(?<before> [a-zA-Z]+\(([a-zA-Z *,]+, |))(?<type>[a-zA-Z]+)(?<after>(| [a-zA-Z *,]+)\))[ \r\n]+where \k<type> : class""), ""${before}${type}*${after}"", 0),
            // protected: abstract TElement GetFirst();
            // protected: virtual TElement GetFirst() = 0;
            (new Regex(@""(?<access>(private|protected|public): )?abstract (?<method>[^;\r\n]+);""), ""${access}virtual ${method} = 0;"", 0),
            // TElement GetFirst();
            // virtual TElement GetFirst() = 0;
            (new Regex(@""([\r\n]+[ ]+)((?!return)[a-zA-Z0-9]+ [a-zA-Z0-9]+\([^\)\r\n]*\))(;[ ]*[\r\n]+)""), ""$1virtual $2 = 0$3"", 1),
            // protected: readonly TreeElement[] _elements;
            // protected: TreeElement _elements[N];
            (new Regex(@""(?<access>(private|protected|public): )?readonly (?<type>[a-zA-Z<>0-9]+)([\[\]]+) (?<name>[_a-zA-Z0-9]+);""), ""${access}${type} ${name}[N];"", 0),
            // protected: readonly TElement Zero;
            // protected: TElement Zero;
            (new Regex(@""(?<access>(private|protected|public): )?readonly (?<type>[a-zA-Z<>0-9]+) (?<name>[_a-zA-Z0-9]+);""), ""${access}${type} ${name};"", 0),
            // internal
            // 
            (new Regex(@""(\W)internal\s+""), ""$1"", 0),
            // static void NotImplementedException(ThrowExtensionRoot root) => throw new NotImplementedException();
            // static void NotImplementedException(ThrowExtensionRoot root) { return throw new NotImplementedException(); }
            (new Regex(@""(^\s+)(private|protected|public)?(: )?(template \<[^>\r\n]+\> )?(static )?(override )?([a-zA-Z0-9]+ )([a-zA-Z0-9]+)\(([^\(\r\n]*)\)\s+=>\s+throw([^;\r\n]+);""), ""$1$2$3$4$5$6$7$8($9) { throw$10; }"", 0),
            // SizeBalancedTree(int capacity) => a = b;
            // SizeBalancedTree(int capacity) { a = b; }
            (new Regex(@""(^\s+)(private|protected|public)?(: )?(template \<[^>\r\n]+\> )?(static )?(override )?(void )?([a-zA-Z0-9]+)\(([^\(\r\n]*)\)\s+=>\s+([^;\r\n]+);""), ""$1$2$3$4$5$6$7$8($9) { $10; }"", 0),
            // int SizeBalancedTree(int capacity) => a;
            // int SizeBalancedTree(int capacity) { return a; }
            (new Regex(@""(^\s+)(private|protected|public)?(: )?(template \<[^>\r\n]+\> )?(static )?(override )?([a-zA-Z0-9]+ )([a-zA-Z0-9]+)\(([^\(\r\n]*)\)\s+=>\s+([^;\r\n]+);""), ""$1$2$3$4$5$6$7$8($9) { return $10; }"", 0),
            // () => Integer<TElement>.Zero,
            // () { return Integer<TElement>.Zero; },
            (new Regex(@""\(\)\s+=>\s+(?<expression>[^(),;\r\n]+(\(((?<parenthesis>\()|(?<-parenthesis>\))|[^();\r\n]*?)*?\))?[^(),;\r\n]*)(?<after>,|\);)""), ""() { return ${expression}; }${after}"", 0),
            // => Integer<TElement>.Zero;
            // { return Integer<TElement>.Zero; }
            (new Regex(@""\)\s+=>\s+([^;\r\n]+?);""), "") { return $1; }"", 0),
            // () { return avlTree.Count; }
            // [&]()-> auto { return avlTree.Count; }
            (new Regex(@""(?<before>, |\()\(\) { return (?<expression>[^;\r\n]+); }""), ""${before}[&]()-> auto { return ${expression}; }"", 0),
            // Count => GetSizeOrZero(Root);
            // GetCount() { return GetSizeOrZero(Root); }
            (new Regex(@""(\W)([A-Z][a-zA-Z]+)\s+=>\s+([^;\r\n]+);""), ""$1Get$2() { return $3; }"", 0),
            // Func<TElement> treeCount
            // std::function<TElement()> treeCount
            (new Regex(@""Func<([a-zA-Z0-9]+)> ([a-zA-Z0-9]+)""), ""std::function<$1()> $2"", 0),
            // Action<TElement> free
            // std::function<void(TElement)> free
            (new Regex(@""Action<([a-zA-Z0-9]+)> ([a-zA-Z0-9]+)""), ""std::function<void($1)> $2"", 0),
            // Predicate<TArgument> predicate
            // std::function<bool(TArgument)> predicate
            (new Regex(@""Predicate<([a-zA-Z0-9]+)> ([a-zA-Z0-9]+)""), ""std::function<bool($1)> $2"", 0),
            // var
            // auto
            (new Regex(@""(\W)var(\W)""), ""$1auto$2"", 0),
            // unchecked
            // 
            (new Regex(@""[\r\n]{2}\s*?unchecked\s*?$""), """", 0),
            // throw new InvalidOperationException
            // throw std::runtime_error
            (new Regex(@""throw new (InvalidOperationException|Exception)""), ""throw std::runtime_error"", 0),
            // void RaiseExceptionIgnoredEvent(Exception exception)
            // void RaiseExceptionIgnoredEvent(const std::exception& exception)
            (new Regex(@""(\(|, )(System\.Exception|Exception)( |\))""), ""$1const std::exception&$3"", 0),
            // EventHandler<Exception>
            // EventHandler<std::exception>
            (new Regex(@""(\W)(System\.Exception|Exception)(\W)""), ""$1std::exception$3"", 0),
            // override void PrintNode(TElement node, StringBuilder sb, int level)
            // void PrintNode(TElement node, StringBuilder sb, int level) override
            (new Regex(@""override ([a-zA-Z0-9 \*\+]+)(\([^\)\r\n]+?\))""), ""$1$2 override"", 0),
            // string
            // const char*
            (new Regex(@""(\W)string(\W)""), ""$1const char*$2"", 0),
            // sbyte
            // std::int8_t
            (new Regex(@""(\W)sbyte(\W)""), ""$1std::int8_t$2"", 0),
            // uint
            // std::uint32_t
            (new Regex(@""(\W)uint(\W)""), ""$1std::uint32_t$2"", 0),
            // char*[] args
            // char* args[]
            (new Regex(@""([_a-zA-Z0-9:\*]?)\[\] ([a-zA-Z0-9]+)""), ""$1 $2[]"", 0),
            // @object
            // object
            (new Regex(@""@([_a-zA-Z0-9]+)""), ""$1"", 0),
            // using Platform.Numbers;
            // 
            (new Regex(@""([\r\n]{2}|^)\s*?using [\.a-zA-Z0-9]+;\s*?$""), """", 0),
            // struct TreeElement { }
            // struct TreeElement { };
            (new Regex(@""(struct|class) ([a-zA-Z0-9]+)(\s+){([\sa-zA-Z0-9;:_]+?)}([^;])""), ""$1 $2$3{$4};$5"", 0),
            // class Program { }
            // class Program { };
            (new Regex(@""(struct|class) ([a-zA-Z0-9]+[^\r\n]*)([\r\n]+(?<indentLevel>[\t ]*)?)\{([\S\s]+?[\r\n]+\k<indentLevel>)\}([^;]|$)""), ""$1 $2$3{$4};$5"", 0),
            // class SizedBinaryTreeMethodsBase : GenericCollectionMethodsBase
            // class SizedBinaryTreeMethodsBase : public GenericCollectionMethodsBase
            (new Regex(@""class ([a-zA-Z0-9]+) : ([a-zA-Z0-9]+)""), ""class $1 : public $2"", 0),
            // class IProperty : ISetter<TValue, TObject>, IProvider<TValue, TObject>
            // class IProperty : public ISetter<TValue, TObject>, IProvider<TValue, TObject>
            (new Regex(@""(?<before>class [a-zA-Z0-9]+ : ((public [a-zA-Z0-9]+(<[a-zA-Z0-9 ,]+>)?, )+)?)(?<inheritedType>(?!public)[a-zA-Z0-9]+(<[a-zA-Z0-9 ,]+>)?)(?<after>(, [a-zA-Z0-9]+(?!>)|[ \r\n]+))""), ""${before}public ${inheritedType}${after}"", 10),
            // Insert scope borders.
            // ref TElement root
            // ~!root!~ref TElement root
            (new Regex(@""(?<definition>(?<= |\()(ref [a-zA-Z0-9]+|[a-zA-Z0-9]+(?<!ref)) (?<variable>[a-zA-Z0-9]+)(?=\)|, | =))""), ""~!${variable}!~${definition}"", 0),
            // Inside the scope of ~!root!~ replace:
            // root
            // *root
            (new Regex(@""(?<definition>~!(?<pointer>[a-zA-Z0-9]+)!~ref [a-zA-Z0-9]+ \k<pointer>(?=\)|, | =))(?<before>((?<!~!\k<pointer>!~)(.|\n))*?)(?<prefix>(\W |\())\k<pointer>(?<suffix>( |\)|;|,))""), ""${definition}${before}${prefix}*${pointer}${suffix}"", 70),
            // Remove scope borders.
            // ~!root!~
            // 
            (new Regex(@""~!(?<pointer>[a-zA-Z0-9]+)!~""), """", 5),
            // ref auto root = ref
            // ref auto root = 
            (new Regex(@""ref ([a-zA-Z0-9]+) ([a-zA-Z0-9]+) = ref(\W)""), ""$1* $2 =$3"", 0),
            // *root = ref left;
            // root = left;
            (new Regex(@""\*([a-zA-Z0-9]+) = ref ([a-zA-Z0-9]+)(\W)""), ""$1 = $2$3"", 0),
            // (ref left)
            // (left)
            (new Regex(@""\(ref ([a-zA-Z0-9]+)(\)|\(|,)""), ""($1$2"", 0),
            //  ref TElement 
            //  TElement* 
            (new Regex(@""( |\()ref ([a-zA-Z0-9]+) ""), ""$1$2* "", 0),
            // ref sizeBalancedTree.Root
            // &sizeBalancedTree->Root
            (new Regex(@""ref ([a-zA-Z0-9]+)\.([a-zA-Z0-9\*]+)""), ""&$1->$2"", 0),
            // ref GetElement(node).Right
            // &GetElement(node)->Right
            (new Regex(@""ref ([a-zA-Z0-9]+)\(([a-zA-Z0-9\*]+)\)\.([a-zA-Z0-9]+)""), ""&$1($2)->$3"", 0),
            // GetElement(node).Right
            // GetElement(node)->Right
            (new Regex(@""([a-zA-Z0-9]+)\(([a-zA-Z0-9\*]+)\)\.([a-zA-Z0-9]+)""), ""$1($2)->$3"", 0),
            // [Fact]\npublic: static void SizeBalancedTreeMultipleAttachAndDetachTest()
            // public: TEST_METHOD(SizeBalancedTreeMultipleAttachAndDetachTest)
            (new Regex(@""\[Fact\][\s\n]+(public: )?(static )?void ([a-zA-Z0-9]+)\(\)""), ""public: TEST_METHOD($3)"", 0),
            // class TreesTests
            // TEST_CLASS(TreesTests)
            (new Regex(@""class ([a-zA-Z0-9]+)Tests""), ""TEST_CLASS($1)"", 0),
            // Assert.Equal
            // Assert::AreEqual
            (new Regex(@""(Assert)\.Equal""), ""$1::AreEqual"", 0),
            // Assert.Throws
            // Assert::ExpectException
            (new Regex(@""(Assert)\.Throws""), ""$1::ExpectException"", 0),
            // $""Argument {argumentName} is null.""
            // ((std::string)""Argument "").append(argumentName).append("" is null."").data()
            (new Regex(@""\$""""(?<left>(\\""""|[^""""\r\n])*){(?<expression>[_a-zA-Z0-9]+)}(?<right>(\\""""|[^""""\r\n])*)""""""), ""((std::string)$\""${left}\"").append(${expression}).append(\""${right}\"").data()"", 10),
            // $""
            // ""
            (new Regex(@""\$""""""), ""\"""", 0),
            // Console.WriteLine(""..."")
            // printf(""...\n"")
            (new Regex(@""Console\.WriteLine\(""""([^""""\r\n]+)""""\)""), ""printf(\""$1\\n\"")"", 0),
            // TElement Root;
            // TElement Root = 0;
            (new Regex(@""(\r?\n[\t ]+)(private|protected|public)?(: )?([a-zA-Z0-9:_]+(?<!return)) ([_a-zA-Z0-9]+);""), ""$1$2$3$4 $5 = 0;"", 0),
            // TreeElement _elements[N];
            // TreeElement _elements[N] = { {0} };
            (new Regex(@""(\r?\n[\t ]+)(private|protected|public)?(: )?([a-zA-Z0-9]+) ([_a-zA-Z0-9]+)\[([_a-zA-Z0-9]+)\];""), ""$1$2$3$4 $5[$6] = { {0} };"", 0),
            // auto path = new TElement[MaxPath];
            // TElement path[MaxPath] = { {0} };
            (new Regex(@""(\r?\n[\t ]+)[a-zA-Z0-9]+ ([a-zA-Z0-9]+) = new ([a-zA-Z0-9]+)\[([_a-zA-Z0-9]+)\];""), ""$1$3 $2[$4] = { {0} };"", 0),
            // private: static readonly ConcurrentBag<std::exception> _exceptionsBag = new ConcurrentBag<std::exception>();
            // private: inline static std::mutex _exceptionsBag_mutex; \n\n private: inline static std::vector<std::exception> _exceptionsBag;
            (new Regex(@""(?<begin>\r?\n?(?<indent>[ \t]+))(?<access>(private|protected|public): )?static readonly ConcurrentBag<(?<argumentType>[^;\r\n]+)> (?<name>[_a-zA-Z0-9]+) = new ConcurrentBag<\k<argumentType>>\(\);""), ""${begin}private: inline static std::mutex ${name}_mutex;"" + Environment.NewLine + Environment.NewLine + ""${indent}${access}inline static std::vector<${argumentType}> ${name};"", 0),
            // public: static IReadOnlyCollection<std::exception> GetCollectedExceptions() { return _exceptionsBag; }
            // public: static std::vector<std::exception> GetCollectedExceptions() { return std::vector<std::exception>(_exceptionsBag); }
            (new Regex(@""(?<access>(private|protected|public): )?static IReadOnlyCollection<(?<argumentType>[^;\r\n]+)> (?<methodName>[_a-zA-Z0-9]+)\(\) { return (?<fieldName>[_a-zA-Z0-9]+); }""), ""${access}static std::vector<${argumentType}> ${methodName}() { return std::vector<${argumentType}>(${fieldName}); }"", 0),
            // public: static event EventHandler<std::exception> ExceptionIgnored = OnExceptionIgnored; ... };
            // ... public: static inline Platform::Delegates::MulticastDelegate<void(void*, const std::exception&)> ExceptionIgnored = OnExceptionIgnored; };
            (new Regex(@""(?<begin>\r?\n(\r?\n)?(?<halfIndent>[ \t]+)\k<halfIndent>)(?<access>(private|protected|public): )?static event EventHandler<(?<argumentType>[^;\r\n]+)> (?<name>[_a-zA-Z0-9]+) = (?<defaultDelegate>[_a-zA-Z0-9]+);(?<middle>(.|\n)+?)(?<end>\r?\n\k<halfIndent>};)""), ""${middle}"" + Environment.NewLine + Environment.NewLine + ""${halfIndent}${halfIndent}${access}static inline Platform::Delegates::MulticastDelegate<void(void*, const ${argumentType}&)> ${name} = ${defaultDelegate};${end}"", 0),
            // Insert scope borders.
            // class IgnoredExceptions { ... private: inline static std::vector<std::exception> _exceptionsBag;
            // class IgnoredExceptions {/*~_exceptionsBag~*/ ... private: inline static std::vector<std::exception> _exceptionsBag;
            (new Regex(@""(?<classDeclarationBegin>\r?\n(?<indent>[\t ]*)class [^{\r\n]+\r\n[\t ]*{)(?<middle>((?!class).|\n)+?)(?<vectorFieldDeclaration>(?<access>(private|protected|public): )inline static std::vector<(?<argumentType>[^;\r\n]+)> (?<fieldName>[_a-zA-Z0-9]+);)""), ""${classDeclarationBegin}/*~${fieldName}~*/${middle}${vectorFieldDeclaration}"", 0),
            // Inside the scope of ~!_exceptionsBag!~ replace:
            // _exceptionsBag.Add(exception);
            // _exceptionsBag.push_back(exception);
            (new Regex(@""(?<scope>/\*~(?<fieldName>[_a-zA-Z0-9]+)~\*/)(?<separator>.|\n)(?<before>((?<!/\*~\k<fieldName>~\*/)(.|\n))*?)\k<fieldName>\.Add""), ""${scope}${separator}${before}${fieldName}.push_back"", 10),
            // Remove scope borders.
            // /*~_exceptionsBag~*/
            // 
            (new Regex(@""/\*~[_a-zA-Z0-9]+~\*/""), """", 0),
            // Insert scope borders.
            // class IgnoredExceptions { ... private: static std::mutex _exceptionsBag_mutex;
            // class IgnoredExceptions {/*~_exceptionsBag~*/ ... private: static std::mutex _exceptionsBag_mutex;
            (new Regex(@""(?<classDeclarationBegin>\r?\n(?<indent>[\t ]*)class [^{\r\n]+\r\n[\t ]*{)(?<middle>((?!class).|\n)+?)(?<mutexDeclaration>private: inline static std::mutex (?<fieldName>[_a-zA-Z0-9]+)_mutex;)""), ""${classDeclarationBegin}/*~${fieldName}~*/${middle}${mutexDeclaration}"", 0),
            // Inside the scope of ~!_exceptionsBag!~ replace:
            // return std::vector<std::exception>(_exceptionsBag);
            // std::lock_guard<std::mutex> guard(_exceptionsBag_mutex); return std::vector<std::exception>(_exceptionsBag);
            (new Regex(@""(?<scope>/\*~(?<fieldName>[_a-zA-Z0-9]+)~\*/)(?<separator>.|\n)(?<before>((?<!/\*~\k<fieldName>~\*/)(.|\n))*?){(?<after>((?!lock_guard)[^{};\r\n])*\k<fieldName>[^;}\r\n]*;)""), ""${scope}${separator}${before}{ std::lock_guard<std::mutex> guard(${fieldName}_mutex);${after}"", 10),
            // Inside the scope of ~!_exceptionsBag!~ replace:
            // _exceptionsBag.Add(exception);
            // std::lock_guard<std::mutex> guard(_exceptionsBag_mutex); \r\n _exceptionsBag.Add(exception);
            (new Regex(@""(?<scope>/\*~(?<fieldName>[_a-zA-Z0-9]+)~\*/)(?<separator>.|\n)(?<before>((?<!/\*~\k<fieldName>~\*/)(.|\n))*?){(?<after>((?!lock_guard)([^{};]|\n))*?\r?\n(?<indent>[ \t]*)\k<fieldName>[^;}\r\n]*;)""), ""${scope}${separator}${before}{"" + Environment.NewLine + ""${indent}std::lock_guard<std::mutex> guard(${fieldName}_mutex);${after}"", 10),
            // Remove scope borders.
            // /*~_exceptionsBag~*/
            // 
            (new Regex(@""/\*~[_a-zA-Z0-9]+~\*/""), """", 0),
            // Insert scope borders.
            // class IgnoredExceptions { ... public: static inline Platform::Delegates::MulticastDelegate<void(void*, const std::exception&)> ExceptionIgnored = OnExceptionIgnored;
            // class IgnoredExceptions {/*~ExceptionIgnored~*/ ... public: static inline Platform::Delegates::MulticastDelegate<void(void*, const std::exception&)> ExceptionIgnored = OnExceptionIgnored;
            (new Regex(@""(?<classDeclarationBegin>\r?\n(?<indent>[\t ]*)class [^{\r\n]+\r\n[\t ]*{)(?<middle>((?!class).|\n)+?)(?<eventDeclaration>(?<access>(private|protected|public): )static inline Platform::Delegates::MulticastDelegate<(?<argumentType>[^;\r\n]+)> (?<name>[_a-zA-Z0-9]+) = (?<defaultDelegate>[_a-zA-Z0-9]+);)""), ""${classDeclarationBegin}/*~${name}~*/${middle}${eventDeclaration}"", 0),
            // Inside the scope of ~!ExceptionIgnored!~ replace:
            // ExceptionIgnored.Invoke(NULL, exception);
            // ExceptionIgnored(NULL, exception);
            (new Regex(@""(?<scope>/\*~(?<eventName>[a-zA-Z0-9]+)~\*/)(?<separator>.|\n)(?<before>((?<!/\*~\k<eventName>~\*/)(.|\n))*?)\k<eventName>\.Invoke""), ""${scope}${separator}${before}${eventName}"", 10),
            // Remove scope borders.
            // /*~ExceptionIgnored~*/
            // 
            (new Regex(@""/\*~[a-zA-Z0-9]+~\*/""), """", 0),
            // Insert scope borders.
            // auto added = new StringBuilder();
            // /*~sb~*/std::string added;
            (new Regex(@""(auto|(System\.Text\.)?StringBuilder) (?<variable>[a-zA-Z0-9]+) = new (System\.Text\.)?StringBuilder\(\);""), ""/*~${variable}~*/std::string ${variable};"", 0),
            // static void Indent(StringBuilder sb, int level)
            // static void Indent(/*~sb~*/StringBuilder sb, int level)
            (new Regex(@""(?<start>, |\()(System\.Text\.)?StringBuilder (?<variable>[a-zA-Z0-9]+)(?<end>,|\))""), ""${start}/*~${variable}~*/std::string& ${variable}${end}"", 0),
            // Inside the scope of ~!added!~ replace:
            // sb.ToString()
            // sb.data()
            (new Regex(@""(?<scope>/\*~(?<variable>[a-zA-Z0-9]+)~\*/)(?<separator>.|\n)(?<before>((?<!/\*~\k<variable>~\*/)(.|\n))*?)\k<variable>\.ToString\(\)""), ""${scope}${separator}${before}${variable}.data()"", 10),
            // sb.AppendLine(argument)
            // sb.append(argument).append('\n')
            (new Regex(@""(?<scope>/\*~(?<variable>[a-zA-Z0-9]+)~\*/)(?<separator>.|\n)(?<before>((?<!/\*~\k<variable>~\*/)(.|\n))*?)\k<variable>\.AppendLine\((?<argument>[^\),\r\n]+)\)""), ""${scope}${separator}${before}${variable}.append(${argument}).append(1, '\\n')"", 10),
            // sb.Append('\t', level);
            // sb.append(level, '\t');
            (new Regex(@""(?<scope>/\*~(?<variable>[a-zA-Z0-9]+)~\*/)(?<separator>.|\n)(?<before>((?<!/\*~\k<variable>~\*/)(.|\n))*?)\k<variable>\.Append\('(?<character>[^'\r\n]+)', (?<count>[^\),\r\n]+)\)""), ""${scope}${separator}${before}${variable}.append(${count}, '${character}')"", 10),
            // sb.Append(argument)
            // sb.append(argument)
            (new Regex(@""(?<scope>/\*~(?<variable>[a-zA-Z0-9]+)~\*/)(?<separator>.|\n)(?<before>((?<!/\*~\k<variable>~\*/)(.|\n))*?)\k<variable>\.Append\((?<argument>[^\),\r\n]+)\)""), ""${scope}${separator}${before}${variable}.append(${argument})"", 10),
            // Remove scope borders.
            // /*~sb~*/
            // 
            (new Regex(@""/\*~[a-zA-Z0-9]+~\*/""), """", 0),
            // Insert scope borders.
            // auto added = new HashSet<TElement>();
            // ~!added!~std::unordered_set<TElement> added;
            (new Regex(@""auto (?<variable>[a-zA-Z0-9]+) = new HashSet<(?<element>[a-zA-Z0-9]+)>\(\);""), ""~!${variable}!~std::unordered_set<${element}> ${variable};"", 0),
            // Inside the scope of ~!added!~ replace:
            // added.Add(node)
            // added.insert(node)
            (new Regex(@""(?<scope>~!(?<variable>[a-zA-Z0-9]+)!~)(?<separator>.|\n)(?<before>((?<!~!\k<variable>!~)(.|\n))*?)\k<variable>\.Add\((?<argument>[a-zA-Z0-9]+)\)""), ""${scope}${separator}${before}${variable}.insert(${argument})"", 10),
            // Inside the scope of ~!added!~ replace:
            // added.Remove(node)
            // added.erase(node)
            (new Regex(@""(?<scope>~!(?<variable>[a-zA-Z0-9]+)!~)(?<separator>.|\n)(?<before>((?<!~!\k<variable>!~)(.|\n))*?)\k<variable>\.Remove\((?<argument>[a-zA-Z0-9]+)\)""), ""${scope}${separator}${before}${variable}.erase(${argument})"", 10),
            // if (added.insert(node)) {
            // if (!added.contains(node)) { added.insert(node);
            (new Regex(@""if \((?<variable>[a-zA-Z0-9]+)\.insert\((?<argument>[a-zA-Z0-9]+)\)\)(?<separator>[\t ]*[\r\n]+)(?<indent>[\t ]*){""), ""if (!${variable}.contains(${argument}))${separator}${indent}{"" + Environment.NewLine + ""${indent}    ${variable}.insert(${argument});"", 0),
            // Remove scope borders.
            // ~!added!~
            // 
            (new Regex(@""~![a-zA-Z0-9]+!~""), """", 5),
            // Insert scope borders.
            // auto random = new System.Random(0);
            // std::srand(0);
            (new Regex(@""[a-zA-Z0-9\.]+ ([a-zA-Z0-9]+) = new (System\.)?Random\(([a-zA-Z0-9]+)\);""), ""~!$1!~std::srand($3);"", 0),
            // Inside the scope of ~!random!~ replace:
            // random.Next(1, N)
            // (std::rand() % N) + 1
            (new Regex(@""(?<scope>~!(?<variable>[a-zA-Z0-9]+)!~)(?<separator>.|\n)(?<before>((?<!~!\k<variable>!~)(.|\n))*?)\k<variable>\.Next\((?<from>[a-zA-Z0-9]+), (?<to>[a-zA-Z0-9]+)\)""), ""${scope}${separator}${before}(std::rand() % ${to}) + ${from}"", 10),
            // Remove scope borders.
            // ~!random!~
            // 
            (new Regex(@""~![a-zA-Z0-9]+!~""), """", 5),
            // Insert method body scope starts.
            // void PrintNodes(TElement node, StringBuilder sb, int level) {
            // void PrintNodes(TElement node, StringBuilder sb, int level) {/*method-start*/
            (new Regex(@""(?<start>\r?\n[\t ]+)(?<prefix>((private|protected|public): )?(virtual )?[a-zA-Z0-9:_]+ )?(?<method>[a-zA-Z][a-zA-Z0-9]*)\((?<arguments>[^\)]*)\)(?<override>( override)?)(?<separator>[ \t\r\n]*)\{(?<end>[^~])""), ""${start}${prefix}${method}(${arguments})${override}${separator}{/*method-start*/${end}"", 0),
            // Insert method body scope ends.
            // {/*method-start*/...}
            // {/*method-start*/.../*method-end*/}
            (new Regex(@""\{/\*method-start\*/(?<body>((?<bracket>\{)|(?<-bracket>\})|[^\{\}]*)+)\}""), ""{/*method-start*/${body}/*method-end*/}"", 0),
            // Inside method bodies replace:
            // GetFirst(
            // this->GetFirst(
            //(new Regex(@""(?<separator>(\(|, |([\W]) |return ))(?<!(->|\* ))(?<method>(?!sizeof)[a-zA-Z0-9]+)\((?!\) \{)""), ""${separator}this->${method}("", 1),
            (new Regex(@""(?<scope>/\*method-start\*/)(?<before>((?<!/\*method-end\*/)(.|\n))*?)(?<separator>[\W](?<!(::|\.|->)))(?<method>(?!sizeof)[a-zA-Z0-9]+)\((?!\) \{)(?<after>(.|\n)*?)(?<scopeEnd>/\*method-end\*/)""), ""${scope}${before}${separator}this->${method}(${after}${scopeEnd}"", 100),
            // Remove scope borders.
            // /*method-start*/
            // 
            (new Regex(@""/\*method-(start|end)\*/""), """", 0),
            // Insert scope borders.
            // const std::exception& ex
            // const std::exception& ex/*~ex~*/
            (new Regex(@""(?<before>\(| )(?<variableDefinition>(const )?(std::)?exception&? (?<variable>[_a-zA-Z0-9]+))(?<after>\W)""), ""${before}${variableDefinition}/*~${variable}~*/${after}"", 0),
            // Inside the scope of ~!ex!~ replace:
            // ex.Message
            // ex.what()
            (new Regex(@""(?<scope>/\*~(?<variable>[_a-zA-Z0-9]+)~\*/)(?<separator>.|\n)(?<before>((?<!/\*~\k<variable>~\*/)(.|\n))*?)\k<variable>\.Message""), ""${scope}${separator}${before}${variable}.what()"", 10),
            // Remove scope borders.
            // /*~ex~*/
            // 
            (new Regex(@""/\*~[_a-zA-Z0-9]+~\*/""), """", 0),
            // throw new ArgumentNullException(argumentName, message);
            // throw std::invalid_argument(((std::string)""Argument "").append(argumentName).append("" is null: "").append(message).append("".""));
            (new Regex(@""throw new ArgumentNullException\((?<argument>[a-zA-Z]*[Aa]rgument[a-zA-Z]*), (?<message>[a-zA-Z]*[Mm]essage[a-zA-Z]*)\);""), ""throw std::invalid_argument(((std::string)\""Argument \"").append(${argument}).append(\"" is null: \"").append(${message}).append(\"".\""));"", 0),
            // throw new ArgumentException(message, argumentName);
            // throw std::invalid_argument(((std::string)""Invalid "").append(argumentName).append("" argument: "").append(message).append("".""));
            (new Regex(@""throw new ArgumentException\((?<message>[a-zA-Z]*[Mm]essage[a-zA-Z]*), (?<argument>[a-zA-Z]*[Aa]rgument[a-zA-Z]*)\);""), ""throw std::invalid_argument(((std::string)\""Invalid \"").append(${argument}).append(\"" argument: \"").append(${message}).append(\"".\""));"", 0),
            // throw new NotSupportedException();
            // throw std::logic_error(""Not supported exception."");
            (new Regex(@""throw new NotSupportedException\(\);""), ""throw std::logic_error(\""Not supported exception.\"");"", 0),
            // throw new NotImplementedException();
            // throw std::logic_error(""Not implemented exception."");
            (new Regex(@""throw new NotImplementedException\(\);""), ""throw std::logic_error(\""Not implemented exception.\"");"", 0),

            // ICounter<int, int> c1;
            // ICounter<int, int>* c1;
            (new Regex(@""(?<abstractType>I[A-Z][a-zA-Z0-9]+(<[^>\r\n]+>)?) (?<variable>[_a-zA-Z0-9]+);""), ""${abstractType}* ${variable};"", 0),
            // (expression)
            // expression
            (new Regex(@""(\(| )\(([a-zA-Z0-9_\*:]+)\)(,| |;|\))""), ""$1$2$3"", 0),
            // (method(expression))
            // method(expression)
            (new Regex(@""(?<firstSeparator>(\(| ))\((?<method>[a-zA-Z0-9_\->\*:]+)\((?<expression>((?<parenthesis>\()|(?<-parenthesis>\))|[a-zA-Z0-9_\->\*:]*)+)(?(parenthesis)(?!))\)\)(?<lastSeparator>(,| |;|\)))""), ""${firstSeparator}${method}(${expression})${lastSeparator}"", 0),
            // return ref _elements[node];
            // return &_elements[node];
            (new Regex(@""return ref ([_a-zA-Z0-9]+)\[([_a-zA-Z0-9\*]+)\];""), ""return &$1[$2];"", 0),
            // null
            // nullptr
            (new Regex(@""(?<before>\r?\n[^""""\r\n]*(""""(\\""""|[^""""\r\n])*""""[^""""\r\n]*)*)(?<=\W)null(?<after>\W)""), ""${before}nullptr${after}"", 10),
            // default
            // 0
            (new Regex(@""(?<before>\r?\n[^""""\r\n]*(""""(\\""""|[^""""\r\n])*""""[^""""\r\n]*)*)(?<=\W)default(?<after>\W)""), ""${before}0${after}"", 10),
            // object x
            // void *x
            (new Regex(@""(?<before>\r?\n[^""""\r\n]*(""""(\\""""|[^""""\r\n])*""""[^""""\r\n]*)*)(?<=\W)([O|o]bject|System\.Object) (?<after>\w)""), ""${before}void *${after}"", 10),
            // <object>
            // <void*>
            (new Regex(@""(?<before>\r?\n[^""""\r\n]*(""""(\\""""|[^""""\r\n])*""""[^""""\r\n]*)*)(?<=\W)(?<!\w )([O|o]bject|System\.Object)(?<after>\W)""), ""${before}void*${after}"", 10),
            // ArgumentNullException
            // std::invalid_argument
            (new Regex(@""(?<before>\r?\n[^""""\r\n]*(""""(\\""""|[^""""\r\n])*""""[^""""\r\n]*)*)(?<=\W)(System\.)?ArgumentNullException(?<after>\W)""), ""${before}std::invalid_argument${after}"", 10),
            // #region Always
            // 
            (new Regex(@""(^|\r?\n)[ \t]*\#(region|endregion)[^\r\n]*(\r?\n|$)""), """", 0),
            // //#define ENABLE_TREE_AUTO_DEBUG_AND_VALIDATION
            //
            (new Regex(@""\/\/[ \t]*\#define[ \t]+[_a-zA-Z0-9]+[ \t]*""), """", 0),
            // #if USEARRAYPOOL\r\n#endif
            // 
            (new Regex(@""#if [a-zA-Z0-9]+\s+#endif""), """", 0),
            // [Fact]
            // 
            (new Regex(@""(?<firstNewLine>\r?\n|\A)(?<indent>[\t ]+)\[[a-zA-Z0-9]+(\((?<expression>((?<parenthesis>\()|(?<-parenthesis>\))|[^()\r\n]*)+)(?(parenthesis)(?!))\))?\][ \t]*(\r?\n\k<indent>)?""), ""${firstNewLine}${indent}"", 5),
            // \n ... namespace
            // namespace
            (new Regex(@""(\S[\r\n]{1,2})?[\r\n]+namespace""), ""$1namespace"", 0),
            // \n ... class
            // class
            (new Regex(@""(\S[\r\n]{1,2})?[\r\n]+class""), ""$1class"", 0),
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
            # out TProduct
            # TProduct
            (r""(?P<before>(<|, ))(in|out) (?P<typeParameter>[a-zA-Z0-9]+)(?P<after>(>|,))"", r""\g<before>\g<typeParameter>\g<after>"", 10),
            # public ...
            # public: ...
            (r""(?P<newLineAndIndent>\r?\n?[ \t]*)(?P<before>[^\{\(\r\n]*)(?P<access>private|protected|public)[ \t]+(?![^\{\(\r\n]*(interface|class|struct)[^\{\(\r\n]*[\{\(\r\n])"", r""\g<newLineAndIndent>\g<access>: \g<before>"", 0),
            # public: static bool CollectExceptions { get; set; }
            # public: inline static bool CollectExceptions;
            (r""(?P<access>(private|protected|public): )(?P<before>(static )?[^\r\n]+ )(?P<name>[a-zA-Z0-9]+) {[^;}]*(?<=\W)get;[^;}]*(?<=\W)set;[^;}]*}"", r""\g<access>inline \g<before>\g<name>;"", 0),
            # public abstract class
            # class
            (r""((public|protected|private|internal|abstract|static) )*(?P<category>interface|class|struct)"", r""\g<category>"", 0),
            # class GenericCollectionMethodsBase<TElement> {
            # template <typename TElement> class GenericCollectionMethodsBase {
            (r""class ([a-zA-Z0-9]+)<([a-zA-Z0-9]+)>([^{]+){"", r""template <typename \2> class \1\3{"", 0),
            # static void TestMultipleCreationsAndDeletions<TElement>(SizedBinaryTreeMethodsBase<TElement> tree, TElement* root)
            # template<typename T> static void TestMultipleCreationsAndDeletions<TElement>(SizedBinaryTreeMethodsBase<TElement> tree, TElement* root)
            (r""static ([a-zA-Z0-9]+) ([a-zA-Z0-9]+)<([a-zA-Z0-9]+)>\(([^\)\r\n]+)\)"", r""template <typename \3> static \1 \2(\4)"", 0),
            # interface IFactory<out TProduct> {
            # template <typename TProduct> class IFactory { public:
            (r""interface (?P<interface>[a-zA-Z0-9]+)<(?P<typeParameters>[a-zA-Z0-9 ,]+)>(?P<whitespace>[^{]+){"", r""template <typename...> class \g<interface>; template <typename \g<typeParameters>> class \g<interface><\g<typeParameters>>\g<whitespace>{\n    public:"", 0),
            # template <typename TObject, TProperty, TValue>
            # template <typename TObject, typename TProperty, TValue>
            (r""(?P<before>template <((, )?typename [a-zA-Z0-9]+)+, )(?P<typeParameter>[a-zA-Z0-9]+)(?P<after>(,|>))"", r""\g<before>typename \g<typeParameter>\g<after>"", 10),
            # Insert markers
            # private: static void BuildExceptionString(this StringBuilder sb, Exception exception, int level)
            # /*~extensionMethod~BuildExceptionString~*/private: static void BuildExceptionString(this StringBuilder sb, Exception exception, int level)
            (r""private: static [^\r\n]+ (?P<name>[a-zA-Z0-9]+)\(this [^\)\r\n]+\)"", r""/*~extensionMethod~\g<name>~*/\0"", 0),
            # Move all markers to the beginning of the file.
            (r""\A(?P<before>[^\r\n]+\r?\n(.|\n)+)(?P<marker>/\*~extensionMethod~(?P<name>[a-zA-Z0-9]+)~\*/)"", r""\g<marker>\g<before>"", 10),
            # /*~extensionMethod~BuildExceptionString~*/...sb.BuildExceptionString(exception.InnerException, level + 1);
            # /*~extensionMethod~BuildExceptionString~*/...BuildExceptionString(sb, exception.InnerException, level + 1);
            (r""(?P<before>/\*~extensionMethod~(?P<name>[a-zA-Z0-9]+)~\*/(.|\n)+\W)(?P<variable>[_a-zA-Z0-9]+)\.(?P=name)\("", r""\g<before>\g<name>(\g<variable>, "", 50),
            # Remove markers
            # /*~extensionMethod~BuildExceptionString~*/
            # 
            (r""/\*~extensionMethod~[a-zA-Z0-9]+~\*/"", r"""", 0),
            # (this 
            # (
            (r""\(this "", r""("", 0),
            # public: static readonly EnsureAlwaysExtensionRoot Always = new EnsureAlwaysExtensionRoot();
            # public:inline static EnsureAlwaysExtensionRoot Always;
            (r""(?P<access>(private|protected|public): )?static readonly (?P<type>[a-zA-Z0-9]+) (?P<name>[a-zA-Z0-9_]+) = new (?P=type)\(\);"", r""\g<access>inline static \g<type> \g<name>;"", 0),
            # public: static readonly string ExceptionContentsSeparator = ""---"";
            # public: inline static const char* ExceptionContentsSeparator = ""---"";
            (r""(?P<access>(private|protected|public): )?static readonly string (?P<name>[a-zA-Z0-9_]+) = \""(?P<string>(\\\""|[^\""\r\n])+)\"";"", r""\g<access>inline static const char* \g<name> = \""\g<string>\"";"", 0),
            # private: const int MaxPath = 92;
            # private: static const int MaxPath = 92;
            (r""(?P<access>(private|protected|public): )?(const|static readonly) (?P<type>[a-zA-Z0-9]+) (?P<name>[_a-zA-Z0-9]+) = (?P<value>[^;\r\n]+);"", r""\g<access>static const \g<type> \g<name> = \g<value>;"", 0),
            #  ArgumentNotNull(EnsureAlwaysExtensionRoot root, TArgument argument) where TArgument : class
            #  ArgumentNotNull(EnsureAlwaysExtensionRoot root, TArgument* argument)
            (r""(?P<before> [a-zA-Z]+\(([a-zA-Z *,]+, |))(?P<type>[a-zA-Z]+)(?P<after>(| [a-zA-Z *,]+)\))[ \r\n]+where (?P=type) : class"", r""\g<before>\g<type>*\g<after>"", 0),
            # protected: abstract TElement GetFirst();
            # protected: virtual TElement GetFirst() = 0;
            (r""(?P<access>(private|protected|public): )?abstract (?P<method>[^;\r\n]+);"", r""\g<access>virtual \g<method> = 0;"", 0),
            # TElement GetFirst();
            # virtual TElement GetFirst() = 0;
            (r""([\r\n]+[ ]+)((?!return)[a-zA-Z0-9]+ [a-zA-Z0-9]+\([^\)\r\n]*\))(;[ ]*[\r\n]+)"", r""\1virtual \2 = 0\3"", 1),
            # protected: readonly TreeElement[] _elements;
            # protected: TreeElement _elements[N];
            (r""(?P<access>(private|protected|public): )?readonly (?P<type>[a-zA-Z<>0-9]+)([\[\]]+) (?P<name>[_a-zA-Z0-9]+);"", r""\g<access>\g<type> \g<name>[N];"", 0),
            # protected: readonly TElement Zero;
            # protected: TElement Zero;
            (r""(?P<access>(private|protected|public): )?readonly (?P<type>[a-zA-Z<>0-9]+) (?P<name>[_a-zA-Z0-9]+);"", r""\g<access>\g<type> \g<name>;"", 0),
            # internal
            # 
            (r""(\W)internal\s+"", r""\1"", 0),
            # static void NotImplementedException(ThrowExtensionRoot root) => throw new NotImplementedException();
            # static void NotImplementedException(ThrowExtensionRoot root) { return throw new NotImplementedException(); }
            (r""(^\s+)(private|protected|public)?(: )?(template \<[^>\r\n]+\> )?(static )?(override )?([a-zA-Z0-9]+ )([a-zA-Z0-9]+)\(([^\(\r\n]*)\)\s+=>\s+throw([^;\r\n]+);"", r""\1\2\3\4\5\6\7\8(\9) { throw\10; }"", 0),
            # SizeBalancedTree(int capacity) => a = b;
            # SizeBalancedTree(int capacity) { a = b; }
            (r""(^\s+)(private|protected|public)?(: )?(template \<[^>\r\n]+\> )?(static )?(override )?(void )?([a-zA-Z0-9]+)\(([^\(\r\n]*)\)\s+=>\s+([^;\r\n]+);"", r""\1\2\3\4\5\6\7\8(\9) { \10; }"", 0),
            # int SizeBalancedTree(int capacity) => a;
            # int SizeBalancedTree(int capacity) { return a; }
            (r""(^\s+)(private|protected|public)?(: )?(template \<[^>\r\n]+\> )?(static )?(override )?([a-zA-Z0-9]+ )([a-zA-Z0-9]+)\(([^\(\r\n]*)\)\s+=>\s+([^;\r\n]+);"", r""\1\2\3\4\5\6\7\8(\9) { return \10; }"", 0),
            # () => Integer<TElement>.Zero,
            # () { return Integer<TElement>.Zero; },
            (r""\(\)\s+=>\s+(?P<expression>[^\(\),;\r\n]+(\(((?P<parenthesis>\()|(?P<parenthesis>\))|[^\(\);\r\n]*?)*?\))?[^\(\),;\r\n]*)(?P<after>,|\);)"", r""() { return \g<expression>; }\g<after>"", 0),
            # => Integer<TElement>.Zero;
            # { return Integer<TElement>.Zero; }
            (r""\)\s+=>\s+([^;\r\n]+?);"", r"") { return \1; }"", 0),
            # () { return avlTree.Count; }
            # [&]()-> auto { return avlTree.Count; }
            (r""(?P<before>, |\()\(\) { return (?P<expression>[^;\r\n]+); }"", r""\g<before>[&]()-> auto { return \g<expression>; }"", 0),
            # Count => GetSizeOrZero(Root);
            # GetCount() { return GetSizeOrZero(Root); }
            (r""(\W)([A-Z][a-zA-Z]+)\s+=>\s+([^;\r\n]+);"", r""\1Get\2() { return \3; }"", 0),
            # Func<TElement> treeCount
            # std::function<TElement()> treeCount
            (r""Func<([a-zA-Z0-9]+)> ([a-zA-Z0-9]+)"", r""std::function<\1()> \2"", 0),
            # Action<TElement> free
            # std::function<void(TElement)> free
            (r""Action<([a-zA-Z0-9]+)> ([a-zA-Z0-9]+)"", r""std::function<void(\1)> \2"", 0),
            # Predicate<TArgument> predicate
            # std::function<bool(TArgument)> predicate
            (r""Predicate<([a-zA-Z0-9]+)> ([a-zA-Z0-9]+)"", r""std::function<bool(\1)> \2"", 0),
            # var
            # auto
            (r""(\W)var(\W)"", r""\1auto\2"", 0),
            # unchecked
            # 
            (r""[\r\n]{2}\s*?unchecked\s*?$"", r"""", 0),
            # throw new InvalidOperationException
            # throw std::runtime_error
            (r""throw new (InvalidOperationException|Exception)"", r""throw std::runtime_error"", 0),
            # void RaiseExceptionIgnoredEvent(Exception exception)
            # void RaiseExceptionIgnoredEvent(const std::exception& exception)
            (r""(\(|, )(System\.Exception|Exception)( |\))"", r""\1const std::exception&\3"", 0),
            # EventHandler<Exception>
            # EventHandler<std::exception>
            (r""(\W)(System\.Exception|Exception)(\W)"", r""\1std::exception\3"", 0),
            # override void PrintNode(TElement node, StringBuilder sb, int level)
            # void PrintNode(TElement node, StringBuilder sb, int level) override
            (r""override ([a-zA-Z0-9 \*\+]+)(\([^\)\r\n]+?\))"", r""\1\2 override"", 0),
            # string
            # const char*
            (r""(\W)string(\W)"", r""\1const char*\2"", 0),
            # sbyte
            # std::int8_t
            (r""(\W)sbyte(\W)"", r""\1std::int8_t\2"", 0),
            # uint
            # std::uint32_t
            (r""(\W)uint(\W)"", r""\1std::uint32_t\2"", 0),
            # char*[] args
            # char* args[]
            (r""([_a-zA-Z0-9:\*]?)\[\] ([a-zA-Z0-9]+)"", r""\1 \2[]"", 0),
            # @object
            # object
            (r""@([_a-zA-Z0-9]+)"", r""\1"", 0),
            # using Platform.Numbers;
            # 
            (r""([\r\n]{2}|^)\s*?using [\.a-zA-Z0-9]+;\s*?$"", r"""", 0),
            # struct TreeElement { }
            # struct TreeElement { };
            (r""(struct|class) ([a-zA-Z0-9]+)(\s+){([\sa-zA-Z0-9;:_]+?)}([^;])"", r""\1 \2\3{\4};\5"", 0),
            # class Program { }
            # class Program { };
            (r""(struct|class) ([a-zA-Z0-9]+[^\r\n]*)([\r\n]+(?P<indentLevel>[\t ]*)?)\{([\S\s]+?[\r\n]+(?P=indentLevel))\}([^;]|$)"", r""\1 \2\3{\4};\5"", 0),
            # class SizedBinaryTreeMethodsBase : GenericCollectionMethodsBase
            # class SizedBinaryTreeMethodsBase : public GenericCollectionMethodsBase
            (r""class ([a-zA-Z0-9]+) : ([a-zA-Z0-9]+)"", r""class \1 : public \2"", 0),
            # class IProperty : ISetter<TValue, TObject>, IProvider<TValue, TObject>
            # class IProperty : public ISetter<TValue, TObject>, IProvider<TValue, TObject>
            (r""(?P<before>class [a-zA-Z0-9]+ : ((public [a-zA-Z0-9]+(<[a-zA-Z0-9 ,]+>)?, )+)?)(?P<inheritedType>(?!public)[a-zA-Z0-9]+(<[a-zA-Z0-9 ,]+>)?)(?P<after>(, [a-zA-Z0-9]+(?!>)|[ \r\n]+))"", r""\g<before>public \g<inheritedType>\g<after>"", 10),
            # Insert scope borders.
            # ref TElement root
            # ~!root!~ref TElement root
            (r""(?P<definition>(?<= |\()(ref [a-zA-Z0-9]+|[a-zA-Z0-9]+(?<!ref)) (?P<variable>[a-zA-Z0-9]+)(?=\)|, | =))"", r""~!\g<variable>!~\g<definition>"", 0),
            # Inside the scope of ~!root!~ replace:
            # root
            # *root
            (r""(?P<definition>~!(?P<pointer>[a-zA-Z0-9]+)!~ref [a-zA-Z0-9]+ (?P=pointer)(?=\)|, | =))(?P<before>((?<!~!(?P=pointer)!~)(.|\n))*?)(?P<prefix>(\W |\())(?P=pointer)(?P<suffix>( |\)|;|,))"", r""\g<definition>\g<before>\g<prefix>*\g<pointer>\g<suffix>"", 70),
            # Remove scope borders.
            # ~!root!~
            # 
            (r""~!(?P<pointer>[a-zA-Z0-9]+)!~"", r"""", 5),
            # ref auto root = ref
            # ref auto root = 
            (r""ref ([a-zA-Z0-9]+) ([a-zA-Z0-9]+) = ref(\W)"", r""\1* \2 =\3"", 0),
            # *root = ref left;
            # root = left;
            (r""\*([a-zA-Z0-9]+) = ref ([a-zA-Z0-9]+)(\W)"", r""\1 = \2\3"", 0),
            # (ref left)
            # (left)
            (r""\(ref ([a-zA-Z0-9]+)(\)|\(|,)"", r""(\1\2"", 0),
            #  ref TElement 
            #  TElement* 
            (r""( |\()ref ([a-zA-Z0-9]+) "", r""\1\2* "", 0),
            # ref sizeBalancedTree.Root
            # &sizeBalancedTree->Root
            (r""ref ([a-zA-Z0-9]+)\.([a-zA-Z0-9\*]+)"", r""&\1->\2"", 0),
            # ref GetElement(node).Right
            # &GetElement(node)->Right
            (r""ref ([a-zA-Z0-9]+)\(([a-zA-Z0-9\*]+)\)\.([a-zA-Z0-9]+)"", r""&\1(\2)->\3"", 0),
            # GetElement(node).Right
            # GetElement(node)->Right
            (r""([a-zA-Z0-9]+)\(([a-zA-Z0-9\*]+)\)\.([a-zA-Z0-9]+)"", r""\1(\2)->\3"", 0),
            # [Fact]\npublic: static void SizeBalancedTreeMultipleAttachAndDetachTest()
            # public: TEST_METHOD(SizeBalancedTreeMultipleAttachAndDetachTest)
            (r""\[Fact\][\s\n]+(public: )?(static )?void ([a-zA-Z0-9]+)\(\)"", r""public: TEST_METHOD(\3)"", 0),
            # class TreesTests
            # TEST_CLASS(TreesTests)
            (r""class ([a-zA-Z0-9]+)Tests"", r""TEST_CLASS(\1)"", 0),
            # Assert.Equal
            # Assert::AreEqual
            (r""(Assert)\.Equal"", r""\1::AreEqual"", 0),
            # Assert.Throws
            # Assert::ExpectException
            (r""(Assert)\.Throws"", r""\1::ExpectException"", 0),
            # $""Argument {argumentName} is null.""
            # ((std::string)""Argument "").append(argumentName).append("" is null."").data()
            (r""\$""""(?P<left>(\\\""|[^\""\r\n])*){(?P<expression>[_a-zA-Z0-9]+)}(?P<right>(\\\""|[^\""\r\n])*)\"""", r""((std::string)$\""\g<left>\"").append(\g<expression>).append(\""\g<right>\"").data()"", 10),
            # $""
            # ""
            (r""\$"""""", r""\"""", 0),
            # Console.WriteLine(""..."")
            # printf(""...\n"")
            (r""Console\.WriteLine\(\""([^\""\r\n]+)\""\)"", r""printf(\""\1\\n\"")"", 0),
            # TElement Root;
            # TElement Root = 0;
            (r""(\r?\n[\t ]+)(private|protected|public)?(: )?([a-zA-Z0-9:_]+(?<!return)) ([_a-zA-Z0-9]+);"", r""\1\2\3\4 \5 = 0;"", 0),
            # TreeElement _elements[N];
            # TreeElement _elements[N] = { {0} };
            (r""(\r?\n[\t ]+)(private|protected|public)?(: )?([a-zA-Z0-9]+) ([_a-zA-Z0-9]+)\[([_a-zA-Z0-9]+)\];"", r""\1\2\3\4 \5[\6] = { {0} };"", 0),
            # auto path = new TElement[MaxPath];
            # TElement path[MaxPath] = { {0} };
            (r""(\r?\n[\t ]+)[a-zA-Z0-9]+ ([a-zA-Z0-9]+) = new ([a-zA-Z0-9]+)\[([_a-zA-Z0-9]+)\];"", r""\1\3 \2[\4] = { {0} };"", 0),
            # private: static readonly ConcurrentBag<std::exception> _exceptionsBag = new ConcurrentBag<std::exception>();
            # private: inline static std::mutex _exceptionsBag_mutex; \n\n private: inline static std::vector<std::exception> _exceptionsBag;
            (r""(?P<begin>\r?\n?(?P<indent>[ \t]+))(?P<access>(private|protected|public): )?static readonly ConcurrentBag<(?P<argumentType>[^;\r\n]+)> (?P<name>[_a-zA-Z0-9]+) = new ConcurrentBag<(?P=argumentType)>\(\);"", r""\g<begin>private: inline static std::mutex \g<name>_mutex;\n\n\g<indent>\g<access>inline static std::vector<\g<argumentType>> \g<name>;"", 0),
            # public: static IReadOnlyCollection<std::exception> GetCollectedExceptions() { return _exceptionsBag; }
            # public: static std::vector<std::exception> GetCollectedExceptions() { return std::vector<std::exception>(_exceptionsBag); }
            (r""(?P<access>(private|protected|public): )?static IReadOnlyCollection<(?P<argumentType>[^;\r\n]+)> (?P<methodName>[_a-zA-Z0-9]+)\(\) { return (?P<fieldName>[_a-zA-Z0-9]+); }"", r""\g<access>static std::vector<\g<argumentType>> \g<methodName>() { return std::vector<\g<argumentType>>(\g<fieldName>); }"", 0),
            # public: static event EventHandler<std::exception> ExceptionIgnored = OnExceptionIgnored; ... };
            # ... public: static inline Platform::Delegates::MulticastDelegate<void(void*, const std::exception&)> ExceptionIgnored = OnExceptionIgnored; };
            (r""(?P<begin>\r?\n(\r?\n)?(?P<halfIndent>[ \t]+)(?P=halfIndent))(?P<access>(private|protected|public): )?static event EventHandler<(?P<argumentType>[^;\r\n]+)> (?P<name>[_a-zA-Z0-9]+) = (?P<defaultDelegate>[_a-zA-Z0-9]+);(?P<middle>(.|\n)+?)(?P<end>\r?\n(?P=halfIndent)};)"", r""\g<middle>\n\n\g<halfIndent>\g<halfIndent>\g<access>static inline Platform::Delegates::MulticastDelegate<void(void*, const \g<argumentType>&)> \g<name> = \g<defaultDelegate>;\g<end>"", 0),
            # Insert scope borders.
            # class IgnoredExceptions { ... private: inline static std::vector<std::exception> _exceptionsBag;
            # class IgnoredExceptions {/*~_exceptionsBag~*/ ... private: inline static std::vector<std::exception> _exceptionsBag;
            (r""(?P<classDeclarationBegin>\r?\n(?P<indent>[\t ]*)class [^{\r\n]+\r\n[\t ]*{)(?P<middle>((?!class).|\n)+?)(?P<vectorFieldDeclaration>(?P<access>(private|protected|public): )inline static std::vector<(?P<argumentType>[^;\r\n]+)> (?P<fieldName>[_a-zA-Z0-9]+);)"", r""\g<classDeclarationBegin>/*~\g<fieldName>~*/\g<middle>\g<vectorFieldDeclaration>"", 0),
            # Inside the scope of ~!_exceptionsBag!~ replace:
            # _exceptionsBag.Add(exception);
            # _exceptionsBag.push_back(exception);
            (r""(?P<scope>/\*~(?P<fieldName>[_a-zA-Z0-9]+)~\*/)(?P<separator>.|\n)(?P<before>((?<!/\*~(?P=fieldName)~\*/)(.|\n))*?)(?P=fieldName)\.Add"", r""\g<scope>\g<separator>\g<before>\g<fieldName>.push_back"", 10),
            # Remove scope borders.
            # /*~_exceptionsBag~*/
            # 
            (r""/\*~[_a-zA-Z0-9]+~\*/"", r"""", 0),
            # Insert scope borders.
            # class IgnoredExceptions { ... private: static std::mutex _exceptionsBag_mutex;
            # class IgnoredExceptions {/*~_exceptionsBag~*/ ... private: static std::mutex _exceptionsBag_mutex;
            (r""(?P<classDeclarationBegin>\r?\n(?P<indent>[\t ]*)class [^{\r\n]+\r\n[\t ]*{)(?P<middle>((?!class).|\n)+?)(?P<mutexDeclaration>private: inline static std::mutex (?P<fieldName>[_a-zA-Z0-9]+)_mutex;)"", r""\g<classDeclarationBegin>/*~\g<fieldName>~*/\g<middle>\g<mutexDeclaration>"", 0),
            # Inside the scope of ~!_exceptionsBag!~ replace:
            # return std::vector<std::exception>(_exceptionsBag);
            # std::lock_guard<std::mutex> guard(_exceptionsBag_mutex); return std::vector<std::exception>(_exceptionsBag);
            (r""(?P<scope>/\*~(?P<fieldName>[_a-zA-Z0-9]+)~\*/)(?P<separator>.|\n)(?P<before>((?<!/\*~(?P=fieldName)~\*/)(.|\n))*?){(?P<after>((?!lock_guard)[^{};\r\n])*(?P=fieldName)[^;}\r\n]*;)"", r""\g<scope>\g<separator>\g<before>{ std::lock_guard<std::mutex> guard(\g<fieldName>_mutex);\g<after>"", 10),
            # Inside the scope of ~!_exceptionsBag!~ replace:
            # _exceptionsBag.Add(exception);
            # std::lock_guard<std::mutex> guard(_exceptionsBag_mutex); \r\n _exceptionsBag.Add(exception);
            (r""(?P<scope>/\*~(?P<fieldName>[_a-zA-Z0-9]+)~\*/)(?P<separator>.|\n)(?P<before>((?<!/\*~(?P=fieldName)~\*/)(.|\n))*?){(?P<after>((?!lock_guard)([^{};]|\n))*?\r?\n(?P<indent>[ \t]*)(?P=fieldName)[^;}\r\n]*;)"", r""\g<scope>\g<separator>\g<before>{\n\g<indent>std::lock_guard<std::mutex> guard(\g<fieldName>_mutex);\g<after>"", 10),
            # Remove scope borders.
            # /*~_exceptionsBag~*/
            # 
            (r""/\*~[_a-zA-Z0-9]+~\*/"", r"""", 0),
            # Insert scope borders.
            # class IgnoredExceptions { ... public: static inline Platform::Delegates::MulticastDelegate<void(void*, const std::exception&)> ExceptionIgnored = OnExceptionIgnored;
            # class IgnoredExceptions {/*~ExceptionIgnored~*/ ... public: static inline Platform::Delegates::MulticastDelegate<void(void*, const std::exception&)> ExceptionIgnored = OnExceptionIgnored;
            (r""(?P<classDeclarationBegin>\r?\n(?P<indent>[\t ]*)class [^{\r\n]+\r\n[\t ]*{)(?P<middle>((?!class).|\n)+?)(?P<eventDeclaration>(?P<access>(private|protected|public): )static inline Platform::Delegates::MulticastDelegate<(?P<argumentType>[^;\r\n]+)> (?P<name>[_a-zA-Z0-9]+) = (?P<defaultDelegate>[_a-zA-Z0-9]+);)"", r""\g<classDeclarationBegin>/*~\g<name>~*/\g<middle>\g<eventDeclaration>"", 0),
            # Inside the scope of ~!ExceptionIgnored!~ replace:
            # ExceptionIgnored.Invoke(NULL, exception);
            # ExceptionIgnored(NULL, exception);
            (r""(?P<scope>/\*~(?P<eventName>[a-zA-Z0-9]+)~\*/)(?P<separator>.|\n)(?P<before>((?<!/\*~(?P=eventName)~\*/)(.|\n))*?)(?P=eventName)\.Invoke"", r""\g<scope>\g<separator>\g<before>\g<eventName>"", 10),
            # Remove scope borders.
            # /*~ExceptionIgnored~*/
            # 
            (r""/\*~[a-zA-Z0-9]+~\*/"", r"""", 0),
            # Insert scope borders.
            # auto added = new StringBuilder();
            # /*~sb~*/std::string added;
            (r""(auto|(System\.Text\.)?StringBuilder) (?P<variable>[a-zA-Z0-9]+) = new (System\.Text\.)?StringBuilder\(\);"", r""/*~\g<variable>~*/std::string \g<variable>;"", 0),
            # static void Indent(StringBuilder sb, int level)
            # static void Indent(/*~sb~*/StringBuilder sb, int level)
            (r""(?P<start>, |\()(System\.Text\.)?StringBuilder (?P<variable>[a-zA-Z0-9]+)(?P<end>,|\))"", r""\g<start>/*~\g<variable>~*/std::string& \g<variable>\g<end>"", 0),
            # Inside the scope of ~!added!~ replace:
            # sb.ToString()
            # sb.data()
            (r""(?P<scope>/\*~(?P<variable>[a-zA-Z0-9]+)~\*/)(?P<separator>.|\n)(?P<before>((?<!/\*~(?P=variable)~\*/)(.|\n))*?)(?P=variable)\.ToString\(\)"", r""\g<scope>\g<separator>\g<before>\g<variable>.data()"", 10),
            # sb.AppendLine(argument)
            # sb.append(argument).append('\n')
            (r""(?P<scope>/\*~(?P<variable>[a-zA-Z0-9]+)~\*/)(?P<separator>.|\n)(?P<before>((?<!/\*~(?P=variable)~\*/)(.|\n))*?)(?P=variable)\.AppendLine\((?P<argument>[^\),\r\n]+)\)"", r""\g<scope>\g<separator>\g<before>\g<variable>.append(\g<argument>).append(1, '\\n')"", 10),
            # sb.Append('\t', level);
            # sb.append(level, '\t');
            (r""(?P<scope>/\*~(?P<variable>[a-zA-Z0-9]+)~\*/)(?P<separator>.|\n)(?P<before>((?<!/\*~(?P=variable)~\*/)(.|\n))*?)(?P=variable)\.Append\('(?P<character>[^'\r\n]+)', (?P<count>[^\),\r\n]+)\)"", r""\g<scope>\g<separator>\g<before>\g<variable>.append(\g<count>, '\g<character>')"", 10),
            # sb.Append(argument)
            # sb.append(argument)
            (r""(?P<scope>/\*~(?P<variable>[a-zA-Z0-9]+)~\*/)(?P<separator>.|\n)(?P<before>((?<!/\*~(?P=variable)~\*/)(.|\n))*?)(?P=variable)\.Append\((?P<argument>[^\),\r\n]+)\)"", r""\g<scope>\g<separator>\g<before>\g<variable>.append(\g<argument>)"", 10),
            # Remove scope borders.
            # /*~sb~*/
            # 
            (r""/\*~[a-zA-Z0-9]+~\*/"", r"""", 0),
            # Insert scope borders.
            # auto added = new HashSet<TElement>();
            # ~!added!~std::unordered_set<TElement> added;
            (r""auto (?P<variable>[a-zA-Z0-9]+) = new HashSet<(?P<element>[a-zA-Z0-9]+)>\(\);"", r""~!\g<variable>!~std::unordered_set<\g<element>> \g<variable>;"", 0),
            # Inside the scope of ~!added!~ replace:
            # added.Add(node)
            # added.insert(node)
            (r""(?P<scope>~!(?P<variable>[a-zA-Z0-9]+)!~)(?P<separator>.|\n)(?P<before>((?<!~!(?P=variable)!~)(.|\n))*?)(?P=variable)\.Add\((?P<argument>[a-zA-Z0-9]+)\)"", r""\g<scope>\g<separator>\g<before>\g<variable>.insert(\g<argument>)"", 10),
            # Inside the scope of ~!added!~ replace:
            # added.Remove(node)
            # added.erase(node)
            (r""(?P<scope>~!(?P<variable>[a-zA-Z0-9]+)!~)(?P<separator>.|\n)(?P<before>((?<!~!(?P=variable)!~)(.|\n))*?)(?P=variable)\.Remove\((?P<argument>[a-zA-Z0-9]+)\)"", r""\g<scope>\g<separator>\g<before>\g<variable>.erase(\g<argument>)"", 10),
            # if (added.insert(node)) {
            # if (!added.contains(node)) { added.insert(node);
            (r""if \((?P<variable>[a-zA-Z0-9]+)\.insert\((?P<argument>[a-zA-Z0-9]+)\)\)(?P<separator>[\t ]*[\r\n]+)(?P<indent>[\t ]*){"", r""if (!\g<variable>.contains(\g<argument>))\g<separator>\g<indent>{\n\g<indent>    \g<variable>.insert(\g<argument>);"", 0),
            # Remove scope borders.
            # ~!added!~
            # 
            (r""~![a-zA-Z0-9]+!~"", r"""", 5),
            # Insert scope borders.
            # auto random = new System.Random(0);
            # std::srand(0);
            (r""[a-zA-Z0-9\.]+ ([a-zA-Z0-9]+) = new (System\.)?Random\(([a-zA-Z0-9]+)\);"", r""~!\1!~std::srand(\3);"", 0),
            # Inside the scope of ~!random!~ replace:
            # random.Next(1, N)
            # (std::rand() % N) + 1
            (r""(?P<scope>~!(?P<variable>[a-zA-Z0-9]+)!~)(?P<separator>.|\n)(?P<before>((?<!~!(?P=variable)!~)(.|\n))*?)(?P=variable)\.Next\((?P<from>[a-zA-Z0-9]+), (?P<to>[a-zA-Z0-9]+)\)"", r""\g<scope>\g<separator>\g<before>(std::rand() % \g<to>) + \g<from>"", 10),
            # Remove scope borders.
            # ~!random!~
            # 
            (r""~![a-zA-Z0-9]+!~"", r"""", 5),
            # Insert method body scope starts.
            # void PrintNodes(TElement node, StringBuilder sb, int level) {
            # void PrintNodes(TElement node, StringBuilder sb, int level) {/*method-start*/
            (r""(?P<start>\r?\n[\t ]+)(?P<prefix>((private|protected|public): )?(virtual )?[a-zA-Z0-9:_]+ )?(?P<method>[a-zA-Z][a-zA-Z0-9]*)\((?P<arguments>[^\)]*)\)(?P<override>( override)?)(?P<separator>[ \t\r\n]*)\{(?P<end>[^~])"", r""\g<start>\g<prefix>\g<method>(\g<arguments>)\g<override>\g<separator>{/*method-start*/\g<end>"", 0),
            # Insert method body scope ends.
            # {/*method-start*/...}
            # {/*method-start*/.../*method-end*/}
            (r""\{/\*method-start\*/(?P<body>((?P<bracket>\{)|(?P<bracket>\})|[^\{\}]*)+)\}"", r""{/*method-start*/\g<body>/*method-end*/}"", 0),
            # Inside method bodies replace:
            # GetFirst(
            # this->GetFirst(
            # (r""(?P<separator>(\(|, |([\W]) |return ))(?<!(->|\* ))(?P<method>(?!sizeof)[a-zA-Z0-9]+)\((?!\) \{)"", r""\g<separator>this->\g<method>("", 1),
            (r""(?P<scope>/\*method-start\*/)(?P<before>((?<!/\*method-end\*/)(.|\n))*?)(?P<separator>[\W](?<!(::|\.|->)))(?P<method>(?!sizeof)[a-zA-Z0-9]+)\((?!\) \{)(?P<after>(.|\n)*?)(?P<scopeEnd>/\*method-end\*/)"", r""\g<scope>\g<before>\g<separator>this->\g<method>(\g<after>\g<scopeEnd>"", 100),
            # Remove scope borders.
            # /*method-start*/
            # 
            (r""/\*method-(start|end)\*/"", r"""", 0),
            # Insert scope borders.
            # const std::exception& ex
            # const std::exception& ex/*~ex~*/
            (r""(?P<before>\(| )(?P<variableDefinition>(const )?(std::)?exception&? (?P<variable>[_a-zA-Z0-9]+))(?P<after>\W)"", r""\g<before>\g<variableDefinition>/*~\g<variable>~*/\g<after>"", 0),
            # Inside the scope of ~!ex!~ replace:
            # ex.Message
            # ex.what()
            (r""(?P<scope>/\*~(?P<variable>[_a-zA-Z0-9]+)~\*/)(?P<separator>.|\n)(?P<before>((?<!/\*~(?P=variable)~\*/)(.|\n))*?)(?P=variable)\.Message"", r""\g<scope>\g<separator>\g<before>\g<variable>.what()"", 10),
            # Remove scope borders.
            # /*~ex~*/
            # 
            (r""/\*~[_a-zA-Z0-9]+~\*/"", r"""", 0),
            # throw new ArgumentNullException(argumentName, message);
            # throw std::invalid_argument(((std::string)""Argument "").append(argumentName).append("" is null: "").append(message).append("".""));
            (r""throw new ArgumentNullException\((?P<argument>[a-zA-Z]*[Aa]rgument[a-zA-Z]*), (?P<message>[a-zA-Z]*[Mm]essage[a-zA-Z]*)\);"", r""throw std::invalid_argument(((std::string)\""Argument \"").append(\g<argument>).append(\"" is null: \"").append(\g<message>).append(\"".\""));"", 0),
            # throw new ArgumentException(message, argumentName);
            # throw std::invalid_argument(((std::string)""Invalid "").append(argumentName).append("" argument: "").append(message).append("".""));
            (r""throw new ArgumentException\((?P<message>[a-zA-Z]*[Mm]essage[a-zA-Z]*), (?P<argument>[a-zA-Z]*[Aa]rgument[a-zA-Z]*)\);"", r""throw std::invalid_argument(((std::string)\""Invalid \"").append(\g<argument>).append(\"" argument: \"").append(\g<message>).append(\"".\""));"", 0),
            # throw new NotSupportedException();
            # throw std::logic_error(""Not supported exception."");
            (r""throw new NotSupportedException\(\);"", r""throw std::logic_error(\""Not supported exception.\"");"", 0),
            # throw new NotImplementedException();
            # throw std::logic_error(""Not implemented exception."");
            (r""throw new NotImplementedException\(\);"", r""throw std::logic_error(\""Not implemented exception.\"");"", 0),

            # (expression)
            # expression
            (r""(\(| )\(([a-zA-Z0-9_\*:]+)\)(,| |;|\))"", r""\1\2\3"", 0),
            # (method(expression))
            # method(expression)
            (r""(?P<firstSeparator>(\(| ))\((?P<method>[a-zA-Z0-9_\->\*:]+)\((?P<expression>((?P<parenthesis>\()|(?<!parenthesis>\))|[a-zA-Z0-9_\->\*:]*)+)(?(parenthesis)(?!))\)\)(?P<lastSeparator>(,| |;|\)))"", r""\g<firstSeparator>\g<method>(\g<expression>)\g<lastSeparator>"", 0),
            # return ref _elements[node];
            # return &_elements[node];
            (r""return ref ([_a-zA-Z0-9]+)\[([_a-zA-Z0-9\*]+)\];"", r""return &\1[\2];"", 0),
            # default
            # 0
            (r""(\W)default(\W)"", r""{\1}0\2"", 0),
            # //#define ENABLE_TREE_AUTO_DEBUG_AND_VALIDATION
            # 
            (r""\/\/[ \t]*\#define[ \t]+[_a-zA-Z0-9]+[ \t]*"", r"""", 0),
            # #if USEARRAYPOOL\r\n#endif
            # 
            (r""#if [a-zA-Z0-9]+\s+#endif"", r"""", 0),
            # [Fact]
            # 
            (r""(?P<firstNewLine>\r?\n|\A)(?P<indent>[\t ]+)\[[a-zA-Z0-9]+(\((?P<expression>((?P<parenthesis>\()|(?<!parenthesis>\))|[^\(\)]*)+)(?(parenthesis)(?!))\))?\][ \t]*(\r?\n(?P=indent))?"", r""\g<firstNewLine>\g<indent>"", 5),
            # \n ... namespace
            # namespace
            (r""(\S[\r\n]{1,2})?[\r\n]+namespace"", r""\1namespace"", 0),
            # \n ... class
            # class
            (r""(\S[\r\n]{1,2})?[\r\n]+class"", r""\n\1class"", 0)
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