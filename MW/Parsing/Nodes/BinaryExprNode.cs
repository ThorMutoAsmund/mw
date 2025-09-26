using Irony.Ast;
using Irony.Interpreter;
using Irony.Interpreter.Ast;
using Irony.Parsing;
using MW.Parsing.ExampleGrammar;

namespace MW.Parsing.Nodes
{
    public class BinaryExprNode : TypedAstNode
    {
        public TypedAstNode Left { get; private set; } = null!;
        public TypedAstNode Right { get; private set; } = null!;
        public string? Op { get; private set; }

        public override void Init(AstContext ctx, ParseTreeNode node)
        {
            base.Init(ctx, node);

            if (node.ChildNodes.Count == 3)
            {
                this.Left = (AddChild("left", node.ChildNodes[0]) as TypedAstNode)!;
                this.Op = node.ChildNodes[1].FindTokenAndGetText();
                this.Right = (AddChild("right", node.ChildNodes[2]) as TypedAstNode)!;

                if (this.Left.Type == AstType.Time || this.Right.Type == AstType.Time)
                {
                    this.Type = AstType.Time;
                }
                else if (this.Left.Type == AstType.Duration || this.Right.Type == AstType.Duration)
                {
                    this.Type = AstType.Duration;
                }
                else
                {
                    this.Type = AstType.Number;
                }
            }
            else if (node.ChildNodes.Count == 1)
            {
                // pass-through (Term/Expr collapsed)
                this.Op = ExprGrammar.PassOperator;
                this.Left = (AddChild("value", node.ChildNodes[0]) as TypedAstNode)!;
                this.Type = this.Left.Type;
            }
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            if (this.Op == "pass")
            {
                return this.Left.Evaluate(thread);
            }

            var l = Convert.ToDouble(this.Left.Evaluate(thread));
            var r = Convert.ToDouble(this.Right.Evaluate(thread));
            
            return Op switch
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
