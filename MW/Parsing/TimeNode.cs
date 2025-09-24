using Irony.Ast;
using Irony.Interpreter;
using Irony.Interpreter.Ast;
using Irony.Parsing;
using System.Globalization;

namespace MW.Parsing
{
    public class TimeNode : TypedAstNode
    {
        public AstNode? Child { get; private set; }
        public override void Init(AstContext ctx, ParseTreeNode node)
        {
            base.Init(ctx, node);

            this.Child = AddChild("value", node.ChildNodes[0]);
            this.Type = AstType.Time;
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            return this.Child!.EvaluateDouble(thread);
        }
    }
}
