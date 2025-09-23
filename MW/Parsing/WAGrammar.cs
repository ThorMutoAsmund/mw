using Irony.Interpreter;
using Irony.Parsing;
using System.Diagnostics;

namespace MW.Parsing
{
    public sealed class WAGrammar : Grammar
    {
        public const string PassOperator = "pass";
        public const string VariablePrefix = "$";
        public const string AssignmentOperator = ":";
        public const string ArgSeparator = ",";
        public const string ExtendOperator = ".";
        public const string StartObject = "{";
        public const string EndObject = "}";
        public const string StartList = "[";
        public const string EndList = "]";
        public const string StartArgs = "(";
        public const string EndArgs = ")";
        public const string NamedArgSuffix = ":";
        public const string BeatSuffix = "b";
        public WAGrammar()
        {
            // Terminals
            var number = new NumberLiteral<NumberNode>("number", NumberOptions.AllowLetterAfter);
            var varId = new IdentifierTerminal<VarIdentNode>("varId");
            var funcId = new IdentifierTerminal<FuncIdentNode>("funcId");
            var namedArgId = new IdentifierTerminal<NamedArgIdentNode>("namedArgId");

            // Non-terminals
            var lines = new NonTerminal<LinesNode>("Lines");
            var line = new TransientNonTerminal("Line");
            var assignment = new NonTerminal<AssignmentNode>("Assign");
            var expr = new NonTerminal<ExprNode>("Expr");
            var compositeFunction = new NonTerminal<CompositeFunctionNode>("CompositeFunction");
            var func = new NonTerminal<FuncNode>("Func");
            var arg = new TransientNonTerminal("Arg");
            var args = new NonTerminal<ArgsNode>("Args");
            var namedArg = new NonTerminal<NamedArgNode>("NamedArg");
            var namedArgs = new NonTerminal<NamedArgsNode>("NamedArgs");
            var beat = new NonTerminal<BeatNode>("Beat");
            var variable = new NonTerminal<VariableNode>("Variable");

            // EBNF-ish rules
            variable.Rule = VariablePrefix + varId;
            beat.Rule = number + BeatSuffix;
            line.Rule = assignment | expr;
            assignment.Rule = variable + AssignmentOperator + arg;
            expr.Rule = arg | compositeFunction;
            func.Rule = funcId + StartArgs + args + EndArgs;
            arg.Rule = beat | number | variable | StartObject + namedArgs + EndObject | StartList + args + EndList;
            namedArg.Rule = namedArgId + NamedArgSuffix + arg;

            MakePlusRule(lines, line);
            MakePlusRule(args, ToTerm(ArgSeparator), arg);
            MakePlusRule(namedArgs, ToTerm(ArgSeparator), namedArg);
            MakePlusRule(compositeFunction, ToTerm(ExtendOperator), func);

            // Punctuation and precedence
            MarkPunctuation(StartObject, EndObject, StartArgs, EndArgs, ArgSeparator, VariablePrefix, 
                AssignmentOperator, 
                ExtendOperator, StartList, EndList, BeatSuffix);
            //RegisterOperators(1, AssignmentOperator);
            //RegisterOperators(2, MultiplicationOperator, DivisionOperator);

            // // line comments (handle \n and \r\n; EOF is handled automatically)
            var lineComment = new CommentTerminal("LineComment", "//", "\n", "\r\n");

            // /* block comments */
            var blockComment = new CommentTerminal("BlockComment", "/*", "*/");

            // Optional: ensure comments win if you also tokenize '/'
            lineComment.Priority = TerminalPriority.High;
            blockComment.Priority = TerminalPriority.High;

            // Tell Irony to ignore them (they won't appear as tokens/AST nodes)
            NonGrammarTerminals.Add(lineComment);
            NonGrammarTerminals.Add(blockComment);

            this.Root = lines;
            this.LanguageFlags = LanguageFlags.CreateAst;
        }

        public static void Test(string input = "")
        {
            input = """
                // Comment before
                $y: 10
                add(2b,$x,$y)
                """;

            var lang = new LanguageData(new WAGrammar());
            var parser = new Parser(lang);
            var tree = parser.Parse(input);

            if (tree.HasErrors())
            {
                foreach (var e in tree.ParserMessages)
                {
                    Console.WriteLine(e);
                }

                return;
            }

            // Walk the parse tree
            Print(tree.Root, 0);
            Console.WriteLine(input);

            var runtime = new LanguageRuntime(lang);
            var app = new ScriptApp(runtime);
            var thread = new ScriptThread(app);

            // optional: pass variables/context to nodes via Globals
            thread.App.Globals["vars"] = new Dictionary<string, object> { ["x"] = 10.0 };
            thread.App.Globals["settings"] = new Dictionary<string, object> { };

            // Evaluate
            var result = tree.Root.Evaluate(thread);
            Console.WriteLine(result);
        }

        static void Print(ParseTreeNode node, int indent)
        {
            Console.WriteLine(new string(' ', indent) + node);

            foreach (var ch in node.ChildNodes)
            {
                Print(ch, indent + 2);
            }
        }
    }
}
