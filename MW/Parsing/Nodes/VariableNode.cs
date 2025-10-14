using Irony.Ast;
using Irony.Interpreter;
using Irony.Interpreter.Ast;
using Irony.Parsing;

namespace MW.Parsing.Nodes
{
    public class VariableNode : TypedAstNode
    {
        public string Name { get; private set; } = string.Empty;
        public override void Init(AstContext ctx, ParseTreeNode node)
        {
            base.Init(ctx, node);

            this.Name = node.ChildNodes[0].FindTokenAndGetText();
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            if (this.Name == Constants.SongVarName)
            {
                this.Type = AstType.Container;
                return Env.Song;
            }

            var vars = (IDictionary<string, (object, AstType)>)thread.App.Globals["vars"];

            if (vars.ContainsKey(this.Name))
            {
                var value = vars[this.Name];
                this.Type = value.Item2;

                return value.Item1;
            }

            this.Type = AstType.Unset;
            return 0;
        }

        public override string ToString() => this.Name;
    }
}
