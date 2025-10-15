using Irony.Ast;
using Irony.Interpreter;
using Irony.Interpreter.Ast;
using Irony.Parsing;

namespace MW.Parsing.Nodes
{
    public class AddNode : TypedAstNode
    {
        public TypedAstNode Child { get; private set; } = null!;
        public TimeNode? Offset { get; private set; }
        public override void Init(AstContext ctx, ParseTreeNode node)
        {
            base.Init(ctx, node);

            if (node.ChildNodes.Count == 2)
            {
                this.Child = (AddChild("value", node.ChildNodes[0]) as TypedAstNode)!;
                this.Offset = (AddChild("offset", node.ChildNodes[1]) as TimeNode)!;
            }
            else
            {
                this.Child = (AddChild("value", node.ChildNodes[0]) as TypedAstNode)!;
            }
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            var result = this.Child.Evaluate(thread);
            var offset = 0D;
            if (this.Offset is not null)
            {
                offset = this.Offset.EvaluateDouble(thread);
            }

            WAParser.Add(result, offset);

            return result;
        }

        public override string ToString() => nameof(AddNode);
    }
}
