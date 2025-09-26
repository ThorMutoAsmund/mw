using Irony.Ast;
using Irony.Interpreter;
using Irony.Interpreter.Ast;
using Irony.Parsing;
using System.Reflection;
using System.Xml.Linq;

namespace MW.Parsing.Nodes
{
    public class LinesNode : TypedAstNode
    {
        private List<TypedAstNode> lines = new();
        public override void Init(AstContext ctx, ParseTreeNode node)
        {
            base.Init(ctx, node);

            var children = node.ChildNodes;
            int arg = 0;
            foreach (var child in children)
            {
                lines.Add((AddChild(role: $"Line{arg++}", child) as TypedAstNode)!);
            }
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            object result = 0;
            foreach (var line in lines)
            {
                try
                {
                    result = line.Evaluate(thread);
                    Type = line.Type;
                }
                catch (RunException ex)
                {
                    line.Error = ex.Message;
                }
            }

            return result;
        }
    }
}
