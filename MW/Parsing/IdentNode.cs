using Irony.Ast;
using Irony.Interpreter;
using Irony.Interpreter.Ast;
using Irony.Parsing;

namespace MW.Parsing
{
    public class IdentNode : AstNode
    {
        public string Name { get; private set; } = string.Empty;
        public override void Init(AstContext ctx, ParseTreeNode node)
        {
            base.Init(ctx, node);

            this.Name = node.Token.Text;
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            var vars = (IDictionary<string, object>)thread.App.Globals["vars"];
            
            return vars[this.Name];
        }

        public override string ToString() => this.Name;
    }
}
