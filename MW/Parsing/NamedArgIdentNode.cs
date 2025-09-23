using Irony.Ast;
using Irony.Interpreter;
using Irony.Interpreter.Ast;
using Irony.Parsing;

namespace MW.Parsing
{
    public class NamedArgIdentNode : AstNode
    {
        public string Name { get; private set; } = string.Empty;
        public override void Init(AstContext ctx, ParseTreeNode node)
        {
            base.Init(ctx, node);

            this.Name = node.Token.Text;
        }

        public override string ToString() => this.Name;
    }
}
