using Irony.Ast;
using Irony.Interpreter;
using Irony.Parsing;

namespace MW.Parsing.Nodes
{
    public class ArgNode : TypedAstNode
    {
        //public string Name 
        //{ 
        //    get
        //    {
        //        if (name == null)
        //        {
        //            return OrigName;
        //        }
        //        return name;
        //    }
        //    set
        //    {
        //        name = value;
        //    }
        //}
        //private string? name = null;
        public string Name { get; private set; } = string.Empty;
        public TypedAstNode Value { get; private set; } = null!;

        private bool setNameInEvaluate = false;
        private ArgsNode? parentNode = null;
        public override void Init(AstContext ctx, ParseTreeNode node)
        {
            base.Init(ctx, node);

            if (node.ChildNodes.Count == 1)
            {
                // Infer name from the type
                this.Value = (AddChild(role: "Value", node.ChildNodes[0]) as TypedAstNode)!;
                this.setNameInEvaluate = true;
            }
            else
            {
                this.Value = (AddChild(role: "Value", node.ChildNodes[1]) as TypedAstNode)!;
                this.Name = node.ChildNodes[0].FindTokenAndGetText().ToLowerInvariant();
            }
        }

        public void SetParent(ArgsNode argsNode)
        {
            this.parentNode = argsNode;
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            // Return the result of the method call, and 0 if it's null
            var value = Value.Evaluate(thread) ?? 0;
            this.Type = Value.Type;
            if (this.setNameInEvaluate)
            {
                this.Name = Value.Type.ToString().ToLowerInvariant();
            }
            var origName = this.Name;

            // Fix name if other args have same name
            if (this.parentNode != null)
            {
                var cnt = 0;
                while (this.parentNode.Children.Any(c => c != this && c.Name == this.Name))
                {
                    cnt++;
                    this.Name = $"{origName}{cnt}";
                }                
            }

            return value;
        }

        public override string ToString() => $"Arg: {Name}";
    }
}
