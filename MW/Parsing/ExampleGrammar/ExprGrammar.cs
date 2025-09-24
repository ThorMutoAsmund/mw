using Irony.Interpreter;
using Irony.Parsing;

namespace MW.Parsing.ExampleGrammar
{
    public sealed class ExprGrammar : Grammar
    {
        public const string PassOperator = "pass";
        public const string PlusOperator = "+";
        public const string MinusOperator = "-";
        public const string MultiplicationOperator = "*";
        public const string DivisionOperator = "/";
        public const string StartParenthesis = "(";
        public const string EndParenthesis = ")";
        
        public ExprGrammar()
        {
            // Terminals
            var number = new NumberLiteral<NumberNode>("number");
            var ident = new IdentifierTerminal<IdentNode>("ident");

            // Non-terminals
            var expr = new NonTerminal<BinaryExprNode>("Expr");
            var term = new NonTerminal<BinaryExprNode>("Term");
            var factor = new TransientNonTerminal("Factor");

            // EBNF-ish rules
            expr.Rule = expr + PlusOperator + term | expr + MinusOperator + term | term;
            term.Rule = term + MultiplicationOperator + factor | term + DivisionOperator + factor | factor;
            factor.Rule = number | ident | StartParenthesis + expr + EndParenthesis;

            // Punctuation and precedence
            MarkPunctuation(StartParenthesis, EndParenthesis);
            RegisterOperators(1, PlusOperator, MinusOperator);
            RegisterOperators(2, MultiplicationOperator, DivisionOperator);

            Root = expr;
            LanguageFlags = LanguageFlags.CreateAst;
        }

        public static void Test(string input = "1 + 2 * (x - 3)")
        {
            Console.WriteLine(input);

            var lang = new LanguageData(new WAGrammar());
            var parser = new Parser(lang);
            var tree = parser.Parse("1 + 2 * (x - 3)");

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
