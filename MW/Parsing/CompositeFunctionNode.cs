using Irony.Ast;
using Irony.Interpreter;
using Irony.Interpreter.Ast;
using Irony.Parsing;

namespace MW.Parsing
{
    public class CompositeFunctionNode : AstNode
    {
        public AstNode? Value { get; private set; }
        public override void Init(AstContext ctx, ParseTreeNode node)
        {
            base.Init(ctx, node);

            this.Value = AddChild(role: "child", node.ChildNodes[0]);
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            return this.Value?.Evaluate(thread) ?? 0;
        }

    }
}
