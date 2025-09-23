using Irony.Ast;
using Irony.Interpreter;
using Irony.Interpreter.Ast;
using Irony.Parsing;
using System.Globalization;

namespace MW.Parsing
{
    public class NumberNode : AstNode
    {
        public double Value { get; private set; }
        public override void Init(AstContext ctx, ParseTreeNode node)
        {
            base.Init(ctx, node);

            this.Value = Convert.ToDouble(node.Token.Value, CultureInfo.InvariantCulture);
        }

        protected override object DoEvaluate(ScriptThread thread) => this.Value;

        public override string ToString() => this.Value.ToString(CultureInfo.InvariantCulture);
    }
}
