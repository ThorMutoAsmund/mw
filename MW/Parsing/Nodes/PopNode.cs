using Irony.Ast;
using Irony.Interpreter;
using Irony.Interpreter.Ast;
using Irony.Parsing;

namespace MW.Parsing.Nodes
{
    public class PopNode : TypedAstNode
    {
        public override void Init(AstContext ctx, ParseTreeNode node)
        {
            base.Init(ctx, node);
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            WAParser.Pop();

            return 0;
        }

        public override string ToString() => nameof(PopNode);
    }
}
