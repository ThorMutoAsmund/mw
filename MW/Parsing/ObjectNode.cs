using Irony.Ast;
using Irony.Interpreter;
using Irony.Interpreter.Ast;
using Irony.Parsing;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace MW.Parsing
{
    public class ObjectNode : TypedAstNode
    {
        public Dictionary<string, object> Elements { get; private set; } = new();
        public List<NamedArgNode> Children { get; private set; } = new ();
        public override void Init(AstContext ctx, ParseTreeNode node)
        {
            base.Init(ctx, node);

            this.Type = AstType.Object;
            var children = node.ChildNodes[0].ChildNodes;
            int arg = 0;
            foreach (var child in children)
            {
                this.Children.Add((AddChild(role: $"Arg{arg++}", child) as NamedArgNode)!);
            }
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            foreach (var child in this.Children)
            {
                this.Elements[child.Name] = child.Arg?.Evaluate(thread) ?? 0;
            }

            return this.Elements;
        }

        public override string ToString() => JsonConvert.SerializeObject(this.Elements);
    }
}
