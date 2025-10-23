using Irony.Ast;
using Irony.Interpreter;
using Irony.Parsing;
using System.Diagnostics;
using System.Reflection;

namespace MW.Parsing.Nodes
{
    public class FuncNode : TypedAstNode
    {
        public string FunctionName { get; private set; } = string.Empty;
        public ArgsNode ArgsNode { get; private set; } = null!;
        public List<ArgNode> Args => ArgsNode?.Children ?? new();
        private static List<(MethodInfo MethodInfo, FunctionAttribute Attr)>? _functions;
        private static List<(MethodInfo MethodInfo, FunctionAttribute Attr)> functions
        {
            get
            {
                if (_functions == null)
                {
                    _functions = FindFunctions();
                }
                return _functions;
            }
        }

        public override void Init(AstContext ctx, ParseTreeNode node)
        {
            base.Init(ctx, node);

            FunctionName = node.ChildNodes[0].FindTokenAndGetText().ToLowerInvariant();
            ArgsNode = (AddChild(role: $"Args", node.ChildNodes[1]) as ArgsNode)!; 
        }

        protected override object DoEvaluate(ScriptThread thread)
        {
            var function = functions.FirstOrDefault(m => m.Attr.Name == FunctionName);
            if (function == default)
            {
                throw new RunException($"Function {FunctionName} not found");
                //this.Type = AstType.Unset;
                //return 0;
            }

            // Return the result of the method call, and 0 if it's null
            try
            {
                var value = function.MethodInfo.Invoke(null,
                    [new MethodContext(this, thread)]);

                this.Type = AstType.Unset;

                switch (function.MethodInfo.ReturnType)
                {
                    case Type t when value == null || t == typeof(void):
                        {
                            return 0;
                        }
                    case Type t when t == typeof(string):
                        {
                            this.Type = AstType.Text;
                            break;
                        }
                    case Type t when t == typeof(int) || t == typeof(double):
                        {
                            this.Type = AstType.Number;
                            break;
                        }
                    case Type t when t == typeof(int) || t == typeof(double):
                        {
                            this.Type = AstType.Number;
                            break;
                        }
                }

                if (function.Attr.AstType != AstType.Unset)
                {
                    this.Type = function.Attr.AstType;
                }

                if (function.Attr.ReturnsTypedValue)
                {
                    var typedValue = value as Tuple<object, AstType>;
                    if (typedValue != null)
                    {
                        this.Type = typedValue.Item2;
                        return typedValue.Item1;
                    }
                    else
                    {
                        throw new ArgumentException($"Function {FunctionName} was expected to return a tuple of (object, AstType) but returned {value?.GetType().Name ?? "null"}");
                    }
                }
                else
                {
                    return value!;
                }
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException != null)
                {
                    throw ex.InnerException;
                }

                throw;
            }
        }

        public override string ToString() => FunctionName;

        private static List<(MethodInfo MethodInfo, FunctionAttribute Method)> FindFunctions()
        {
            var asm = Assembly.GetExecutingAssembly();

            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

            return asm
                .GetTypes()
                .SelectMany(t => t.GetMethods(flags))
                .Select(m => (Method: m, Attr: m.GetCustomAttribute<FunctionAttribute>(inherit: false)!))
                .Where(x => x.Attr != null && !x.Attr.IsCommandLine).ToList();
        }

    }
}
