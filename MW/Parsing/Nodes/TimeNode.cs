using Irony.Ast;
using Irony.Interpreter;
using Irony.Interpreter.Ast;
using Irony.Parsing;

namespace MW.Parsing.Nodes
{
    public class TimeNode : TypedAstNode
    {
        public TypedAstNode Child { get; private set; } = null!;
        public override void Init(AstContext ctx, ParseTreeNode node)
        {
            base.Init(ctx, node);

            this.Child = (AddChild("value", node.ChildNodes[0]) as TypedAstNode)!;
            this.Type = AstType.Time;
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            return this.Child.EvaluateDouble(thread);
        }
    }
}
