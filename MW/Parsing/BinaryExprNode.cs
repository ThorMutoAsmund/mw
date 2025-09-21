using Irony.Ast;
using Irony.Interpreter;
using Irony.Interpreter.Ast;
using Irony.Parsing;

namespace MW.Parsing
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
                this.Left = AddChild("left", node.ChildNodes[0]);
                this.Op = node.ChildNodes[1].FindTokenAndGetText();
                this.Right = AddChild("right", node.ChildNodes[2]);
            }
            else if (node.ChildNodes.Count == 1)
            {
                // pass-through (Term/Expr collapsed)
                this.Op = ExprGrammar.PassOperator;
                this.Left = AddChild("value", node.ChildNodes[0]);
            }
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            if (this.Op == "pass")
            {
                return this.Left!.Evaluate(thread);
            }

            var l = Convert.ToDouble(this.Left!.Evaluate(thread));
            var r = Convert.ToDouble(this.Right!.Evaluate(thread));
            
            return this.Op switch
            {
                ExprGrammar.PlusOperator => l + r,
                ExprGrammar.MinusOperator => l - r,
                ExprGrammar.MultiplicationOperator => l * r,
                ExprGrammar.DivisionOperator => l / r,
                
                _ => throw new NotSupportedException($"op '{this.Op}'")
            };
        }

        public override string ToString() => this.Op ?? "val";
    }
}
