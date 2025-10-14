using Irony.Ast;
using Irony.Interpreter;
using Irony.Interpreter.Ast;
using Irony.Parsing;
using MW.Audio;
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
            Sample? result = null;
            foreach (var line in lines)
            {
                try
                {
                    var lineResult = line.Evaluate(thread);
                    this.Type = line.Type;
                    if (this.Type == AstType.Sample)
                    {
                        result = lineResult as Sample;
                    }
                }
                catch (RunException ex)
                {
                    line.Error = ex.Message;
                }
            }

            return result == null ? 0 : result;
        }
    }
}
