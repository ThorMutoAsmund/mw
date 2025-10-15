using Irony.Ast;
using Irony.Interpreter;
using Irony.Parsing;

namespace MW.Parsing.Nodes
{
    public class TextNode : TypedAstNode
    {
        public string Value { get; private set; } = string.Empty;
        public override void Init(AstContext ctx, ParseTreeNode node)
        {
            base.Init(ctx, node);

            this.Type = AstType.Text;
            this.Value = Convert.ToString(node.Token.Value) ?? string.Empty;
        }

        protected override object DoEvaluate(ScriptThread thread) => this.Value;

        public override string ToString() => this.Value;
    }
}

