using Irony.Ast;
using Irony.Interpreter;
using Irony.Interpreter.Ast;
using Irony.Parsing;
using System.Globalization;

namespace MW.Parsing.Nodes
{
    public class BeatNode : TypedAstNode
    {
        public NumberNode Child { get; private set; } = null!;
        public override void Init(AstContext ctx, ParseTreeNode node)
        {
            base.Init(ctx, node);

            Child = (AddChild("value", node.ChildNodes[0]) as NumberNode)!;
            Type = AstType.Duration;
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            var settings = (IDictionary<string, object>)thread.App.Globals["settings"];

            var bmp = settings.ContainsKey(Constants.BMP) ?
                Convert.ToDouble(settings[Constants.BMP]) : Constants.BMPDefault;

            return Child.EvaluateDouble(thread) * 60.0 / bmp;
        }
    }
}
