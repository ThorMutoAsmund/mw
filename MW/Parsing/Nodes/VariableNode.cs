using Irony.Ast;
using Irony.Interpreter;
using Irony.Interpreter.Ast;
using Irony.Parsing;
using MW.Audio;

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
            else if (this.Name == Constants.OutputVarName)
            {
                if (WAParser.Output is Sample sample)
                {
                    this.Type = AstType.Sample;
                    return sample;
                }
                else if (WAParser.Output is Container container)
                {
                    this.Type = AstType.Container;
                    return container;
                }

                this.Type = AstType.Unset;
                return 0;
            }
            else if (this.Name == Constants.LastVarName)
            {
                if (WAParser.CurrentAudioSource is Sample sample)
                {
                    this.Type = AstType.Sample;
                    return sample;
                }
                else if (WAParser.CurrentAudioSource is Container container)
                {
                    this.Type = AstType.Container;
                    return container;
                }

                this.Type = AstType.Unset;
                return 0;
            }
            else if (this.Name == Constants.HeadVarName)
            {
                if (WAParser.Head is Sample sample)
                {
                    this.Type = AstType.Sample;
                    return sample;
                }
                else if (WAParser.Head is Container container)
                {
                    this.Type = AstType.Container;
                    return container;
                }

                this.Type = AstType.Unset;
                return 0;
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
