using Irony.Ast;
using Irony.Interpreter;
using Irony.Parsing;

namespace MW.Parsing.Nodes
{
    public class ArgNode : TypedAstNode
    {
        public string Name 
        { 
            get
            {
                if (name == null)
                {
                    return OrigName;
                }
                return name;
            }
            set
            {
                name = value;
            }
        }
        private string? name = null;
        public string OrigName { get; private set; } = string.Empty;
        public TypedAstNode Value { get; private set; } = null!;

        public override void Init(AstContext ctx, ParseTreeNode node)
        {
            base.Init(ctx, node);

            if (node.ChildNodes.Count == 1)
            {
                // Infer name from the type
                Value = (AddChild(role: "Value", node.ChildNodes[0]) as TypedAstNode)!;
                OrigName = Value.Type.ToString().ToLowerInvariant();
            }
            else
            {
                Value = (AddChild(role: "Value", node.ChildNodes[1]) as TypedAstNode)!;
                OrigName = node.ChildNodes[0].FindTokenAndGetText().ToLowerInvariant();
            }
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            // Return the result of the method call, and 0 if it's null
            var value = Value.Evaluate(thread) ?? 0;
            Type = Value.Type;

            return value;
        }

        public override string ToString() => Name;
    }
}
