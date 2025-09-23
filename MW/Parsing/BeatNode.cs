using Irony.Ast;
using Irony.Interpreter;
using Irony.Interpreter.Ast;
using Irony.Parsing;
using System.Globalization;

namespace MW.Parsing
{
    public class BeatNode : AstNode
    {
        public AstNode? Child { get; private set; }
        public override void Init(AstContext ctx, ParseTreeNode node)
        {
            base.Init(ctx, node);

            this.Child = AddChild("value", node.ChildNodes[0]);
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            var settings = (IDictionary<string, object>)thread.App.Globals["settings"];

            var bmp = settings.ContainsKey(Constants.BMP) ?
                Convert.ToDouble(settings[Constants.BMP]) : Constants.BMPDefault;

            return this.Child!.EvaluateDouble(thread) * 60.0 / bmp;
        }
    }
}
