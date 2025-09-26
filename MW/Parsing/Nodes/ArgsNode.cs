using Irony.Ast;
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

                var cnt = 0;
                while (Children.Any(c => c.Name == astNode!.Name))
                {
                    cnt++;
                    astNode.Name = $"{astNode.OrigName}{cnt}";
                }
                astNode.Role = astNode.Name;

                Children.Add(astNode);
            }
        }
    }
}
