using Irony.Ast;
using Irony.Interpreter;
using Irony.Interpreter.Ast;
using Irony.Parsing;
using MW.Audio;
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

            this.VariableName = node.ChildNodes[0].FindTokenAndGetText().ToLowerInvariant();
            this.Operand = (AddChild(role: $"Operand", node.ChildNodes[1]) as TypedAstNode)!;
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            var value = this.Operand.Evaluate(thread);

            if (this.VariableName == Constants.OutputVarName)
            {
                if (this.Operand.Type != AstType.Sample)
                {
                    throw new RunException($"Cannot set a {this.Operand.Type} as output");
                }

                if (value is Sample sample)
                {
                    WAParser.SetOutput(sample);
                }
            }

            var vars = (IDictionary<string, (object, AstType)>)thread.App.Globals["vars"];

            vars[this.VariableName] = (value, this.Operand.Type);

            this.Type = this.Operand.Type;

            return vars[this.VariableName].Item1;
        }
    }
}
