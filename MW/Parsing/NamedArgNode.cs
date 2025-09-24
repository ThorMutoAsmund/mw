using Irony.Ast;
using Irony.Interpreter;
using Irony.Interpreter.Ast;
using Irony.Parsing;

namespace MW.Parsing
{
    public class NamedArgNode : TypedAstNode
    {
        public string Name { get; private set; } = string.Empty;
        public TypedAstNode? Arg { get; private set; }
        public override AstType Type 
        {
            get => this.Arg?.Type ?? AstType.Unset;

            protected set { }
        }
        public override void Init(AstContext ctx, ParseTreeNode node)
        {
            base.Init(ctx, node);

            this.Name = node.ChildNodes[0].Token.ValueString.ToLowerInvariant();
            this.Arg = AddChild(role: "Variable", node.ChildNodes[1]) as TypedAstNode;
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            return this.Arg!.Evaluate(thread);
        }

        public override string ToString() => this.Name;
    }
}
