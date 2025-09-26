using Irony.Ast;
using Irony.Interpreter;
using Irony.Interpreter.Ast;
using Irony.Parsing;
using System.Globalization;

namespace MW.Parsing.Nodes
{
    public class SecondsNode : TypedAstNode
    {
        public NumberNode Child { get; private set; } = null!;
        public override void Init(AstContext ctx, ParseTreeNode node)
        {
            base.Init(ctx, node);

            this.Child = (AddChild("value", node.ChildNodes[0]) as NumberNode)!;
            this.Type = AstType.Duration;
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            return this.Child.EvaluateDouble(thread);
        }
    }
}
