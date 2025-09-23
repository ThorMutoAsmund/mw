using Irony.Ast;
using Irony.Interpreter;
using Irony.Interpreter.Ast;
using Irony.Parsing;

namespace MW.Parsing.ExampleGrammar
{
    public class BinaryExprNode : AstNode
    {
        public AstNode? Left { get; private set; }
        public AstNode? Right { get; private set; }
        public string? Op { get; private set; }

        public override void Init(AstContext ctx, ParseTreeNode node)
        {
            base.Init(ctx, node);

            if (node.ChildNodes.Count == 3)
            {
                Left = AddChild("left", node.ChildNodes[0]);
                Op = node.ChildNodes[1].FindTokenAndGetText();
                Right = AddChild("right", node.ChildNodes[2]);
            }
            else if (node.ChildNodes.Count == 1)
            {
                // pass-through (Term/Expr collapsed)
                Op = ExprGrammar.PassOperator;
                Left = AddChild("value", node.ChildNodes[0]);
            }
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            if (Op == "pass")
            {
                return Left!.Evaluate(thread);
            }

            var l = Convert.ToDouble(Left!.Evaluate(thread));
            var r = Convert.ToDouble(Right!.Evaluate(thread));
            
            return Op switch
            {
                ExprGrammar.PlusOperator => l + r,
                ExprGrammar.MinusOperator => l - r,
                ExprGrammar.MultiplicationOperator => l * r,
                ExprGrammar.DivisionOperator => l / r,
                
                _ => throw new NotSupportedException($"op '{Op}'")
            };
        }

        public override string ToString() => Op ?? "val";
    }
}
