using Irony.Ast;
using Irony.Interpreter;
using Irony.Interpreter.Ast;
using Irony.Parsing;
using System.Xml.Linq;

namespace MW.Parsing
{
    public class LinesNode : AstNode
    {
        private List<AstNode> lines = new();
        public override void Init(AstContext ctx, ParseTreeNode node)
        {
            base.Init(ctx, node);

            var children = node.ChildNodes;
            int arg = 0;
            foreach (var child in children)
            {
                this.lines.Add(AddChild(role: $"Line{arg++}", child));
            }
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            object result = 0;
            foreach (var line in this.lines)
            {
                result = line.Evaluate(thread);
            }

            return result;
        }
    }
}
