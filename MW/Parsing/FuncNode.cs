using Irony.Ast;
using Irony.Interpreter;
using Irony.Interpreter.Ast;
using Irony.Parsing;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace MW.Parsing
{
    public class FuncNode : AstNode
    {
        public string MethodName { get; private set; } = string.Empty;
        public List<TypedAstNode?> Args { get; private set; } = new();
        private static List<(MethodInfo MethodInfo, FunctionAttribute Method)>? _methods;
        private static List<(MethodInfo MethodInfo, FunctionAttribute Method)> methods
        {
            get
            {
                if (_methods == null)
                {
                    _methods = FindMethods();
                }
                return _methods;
            }
        }

        public override void Init(AstContext ctx, ParseTreeNode node)
        {
            base.Init(ctx, node);

            this.MethodName = node.ChildNodes[0].Token.ValueString.ToLowerInvariant();
            var children = node.ChildNodes[1].ChildNodes;
            int arg = 0;
            foreach (var child in children)
            {
                this.Args.Add(AddChild(role: $"Arg{arg++}", child) as TypedAstNode);
            }
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            var method = methods.FirstOrDefault(m => m.Method.Name == this.MethodName);
            if (method == default)
            {
                Show.Error($"Method not found: {this.MethodName}");
                return 0;
            }

            return method.MethodInfo.Invoke(null, [thread, this.Args]) ?? 0;
        }

        public override string ToString() => this.MethodName;

        private static List<(MethodInfo MethodInfo, FunctionAttribute Method)> FindMethods()
        {
            var asm = Assembly.GetExecutingAssembly();

            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

            return asm
                .GetTypes()
                .SelectMany(t => t.GetMethods(flags))
                .Select(m => (Method: m, Attr: m.GetCustomAttribute<FunctionAttribute>(inherit: false)!))
                .Where(x => x.Attr != null).ToList();
        }

    }
}
