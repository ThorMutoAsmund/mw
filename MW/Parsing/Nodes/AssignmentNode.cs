using Irony.Ast;
using Irony.Interpreter;
using Irony.Interpreter.Ast;
using Irony.Parsing;
using System.Xml.Linq;

namespace MW.Parsing.Nodes
{
    public class AssignmentNode : TypedAstNode
    {
        public string VariableName { get; private set; } = null!;
        public TypedAstNode Operand { get; private set; } = null!;
        public override void Init(AstContext ctx, ParseTreeNode node)
        {
            base.Init(ctx, node);

            VariableName = node.ChildNodes[0].FindTokenAndGetText().ToLowerInvariant();
            Operand = (AddChild(role: $"Operand", node.ChildNodes[1]) as TypedAstNode)!;
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            var vars = (IDictionary<string, (object, AstType)>)thread.App.Globals["vars"];

            var value = this.Operand.Evaluate(thread);
            vars[VariableName] = (value, Operand.Type);

            Type = Operand.Type;

            return vars[VariableName];
        }
    }
}
