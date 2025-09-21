using Irony.Interpreter;
using Irony.Parsing;

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
            var ident = new IdentifierTerminal<IdentNode>("ident");

            // Non-terminals
            var lines = new NonTerminal<LinesNode>("Lines");
            var line = new NonTerminal<LineNode>("Line");
            var assignment = new NonTerminal<AssignmentNode>("Assign");
            var variable = new TransientNonTerminal("Var");
            var expr = new NonTerminal<ExprNode>("Expr");
            var exprPart = new NonTerminal<ExprPartNode>("ExprPart");
            var args = new NonTerminal<ArgsNode>("Args");
            var arg = new TransientNonTerminal("Arg");
            var namedArgs = new NonTerminal<NamedArgsNode>("NamedArgs");
            var namedArg = new NonTerminal<NamedArgNode>("NamedArg");
            var beat = new NonTerminal<BeatNode>("Beat");


            // EBNF-ish rules
            beat.Rule = number | beat + BeatSuffix;
            lines.Rule = line + lines | line;
            line.Rule = assignment | expr;
            assignment.Rule = variable + AssignmentOperator + expr;
            variable.Rule = VariablePrefix + ident;
            expr.Rule = exprPart + ExtendOperator + exprPart | exprPart;
            exprPart.Rule = ident + StartArgs + args + EndArgs;
            args.Rule = arg + ArgSeparator + args | arg;
            arg.Rule = beat | variable | StartObject + namedArgs + EndObject | StartList + args + EndList;
            namedArgs.Rule = namedArg + ArgSeparator + namedArgs | namedArg;
            namedArg.Rule = ident + NamedArgSuffix + arg;

            // Punctuation and precedence
            MarkPunctuation(StartObject, EndObject, StartArgs, EndArgs, ArgSeparator, VariablePrefix, AssignmentOperator, 
                ExtendOperator, StartList, EndList);
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
                $a: test(2, 3, 4.2, 5b )
                  .show(3, 4)                
                fix(4,5) // Comment after
                fix(3,4) /* block comment */ .extend({start: 2, end: 3, jump: [10,20,30], pattern: {a:2, b:3, c:4}})
                                
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

            // Evaluate
            var result = tree.Root.EvaluateDouble(thread);
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
