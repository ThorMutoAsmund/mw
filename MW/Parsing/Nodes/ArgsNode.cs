using Irony.Ast;
using Irony.Interpreter;
using Irony.Parsing;

namespace MW.Parsing.Nodes
{
    public class ArgsNode : TypedAstNode
    {
        public List<ArgNode> Children { get; private set; } = new();

        public override void Init(AstContext ctx, ParseTreeNode node)
        {
            base.Init(ctx, node);

            int arg = 0;
            foreach (var child in node.ChildNodes)
            {
                var astNode = (AddChild(role: $"Arg{arg++}", child) as ArgNode)!;
                astNode.SetParent(this);
                Children.Add(astNode);
            }
        }

        public override string ToString()
        {
            return "Args";
        }
    }
}
