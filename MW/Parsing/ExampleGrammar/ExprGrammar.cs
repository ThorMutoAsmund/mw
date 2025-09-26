using Irony.Interpreter;
using Irony.Parsing;
using MW.Parsing.Nodes;

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
            NumberLiteral<NumberNode>  number = new("number");
            IdentifierTerminal<IdentNode>  ident = new("ident");

            // Non-terminals
            NonTerminal<BinaryExprNode>  expr = new("Expr");
            NonTerminal<BinaryExprNode>  term = new("Term");
            TransientNonTerminal factor = new("Factor");

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

            LanguageData lang = new(new WAParser());
            Parser parser = new (lang);
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

            LanguageRuntime runtime = new(lang);
            ScriptApp app = new(runtime);
            ScriptThread thread = new(app);

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
