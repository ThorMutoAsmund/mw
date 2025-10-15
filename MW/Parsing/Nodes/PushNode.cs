using Irony.Ast;
using Irony.Interpreter;
using Irony.Interpreter.Ast;
using Irony.Parsing;

namespace MW.Parsing.Nodes
{
    public class PushNode : TypedAstNode
    {
        public override void Init(AstContext ctx, ParseTreeNode node)
        {
            base.Init(ctx, node);
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            WAParser.Push();
            return 0;
        }

        public override string ToString() => nameof(PushNode);
    }
}
