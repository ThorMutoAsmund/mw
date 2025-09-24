using Irony.Ast;
using Irony.Interpreter;
using Irony.Interpreter.Ast;
using Irony.Parsing;
using System.Globalization;

namespace MW.Parsing
{
    public class StringNode : TypedAstNode
    {
        public string Value { get; private set; } = string.Empty;
        public override void Init(AstContext ctx, ParseTreeNode node)
        {
            base.Init(ctx, node);

            this.Type = AstType.String;
            this.Value = Convert.ToString(node.Token.Value) ?? string.Empty;
        }

        protected override object DoEvaluate(ScriptThread thread) => this.Value;

        public override string ToString() => this.Value;
    }
}

