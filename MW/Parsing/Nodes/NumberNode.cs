using Irony.Ast;
using Irony.Interpreter;
using Irony.Interpreter.Ast;
using Irony.Parsing;
using System.Globalization;

namespace MW.Parsing.Nodes
{
    public class NumberNode : TypedAstNode
    {
        public double Value { get; private set; }
        public override void Init(AstContext ctx, ParseTreeNode node)
        {
            base.Init(ctx, node);

            Type = AstType.Number;
            Value = Convert.ToDouble(node.Token.Value, CultureInfo.InvariantCulture);
        }

        protected override object DoEvaluate(ScriptThread thread) => Value;

        public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
    }
}
