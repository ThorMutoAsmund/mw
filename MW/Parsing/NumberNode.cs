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

            var value = Convert.ToString(node.Token.Value);
            if (value == null)
            {
                this.Value = 0;
            }
            else
            {
                if (value.EndsWith("b"))
                {
                    this.Value = -Convert.ToDouble(value.Substring(0, value.Length - 1), CultureInfo.InvariantCulture);
                }
                else
                {
                    this.Value = Convert.ToDouble(value, CultureInfo.InvariantCulture);
                }
            }
        }

        protected override object DoEvaluate(ScriptThread thread) => this.Value;

        public override string ToString() => this.Value.ToString(CultureInfo.InvariantCulture);

    }
}
