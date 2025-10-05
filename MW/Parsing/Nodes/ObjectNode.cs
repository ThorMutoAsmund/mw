using Irony.Ast;
using Irony.Interpreter;
using Irony.Parsing;
using Newtonsoft.Json;

namespace MW.Parsing.Nodes
{
    public class ObjectNode : TypedAstNode
    {
        public Dictionary<string, object> Elements { get; private set; } = new();
        public ArgsNode ArgsNode { get; private set; } = null!;

        public override void Init(AstContext ctx, ParseTreeNode node)
        {
            base.Init(ctx, node);

            this.Type = AstType.Object;
            this.ArgsNode = (AddChild(role: $"Args", node.ChildNodes[0]) as ArgsNode)!;
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            foreach (var child in this.ArgsNode.Children)
            {
                this.Elements[child.Name] = child.Value.Evaluate(thread) ?? 0;
            }

            return this.Elements;
        }

        public override string ToString() => JsonConvert.SerializeObject(Elements);
    }
}
