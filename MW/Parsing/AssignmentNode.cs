using Irony.Ast;
using Irony.Interpreter;
using Irony.Interpreter.Ast;
using Irony.Parsing;
using System.Xml.Linq;

namespace MW.Parsing
{
    public class AssignmentNode : AstNode
    {
        public AstNode? Variable { get; private set; }
        public AstNode? Expr { get; private set; }
        public override void Init(AstContext ctx, ParseTreeNode node)
        {
            base.Init(ctx, node);

            this.Variable = AddChild(role: $"Var", node.ChildNodes[0]);
            this.Expr = AddChild(role: $"Expr", node.ChildNodes[1]);
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            var variableName = (this.Variable as VariableNode)!.Name.ToLowerInvariant();
            var vars = (IDictionary<string, object>)thread.App.Globals["vars"];

            vars[variableName] = this.Expr!.Evaluate(thread);

            return vars[variableName];
        }
    }
}
