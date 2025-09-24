using Irony.Interpreter;
using Irony.Interpreter.Ast;
using Irony.Parsing;
using MW.Parsing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MW.Functions
{
    public static class Misc
    {
        [Function(name: "show", description: "Display the arguments")]
        public static void Show(ScriptThread thread, List<TypedAstNode> args)
        {
            foreach (var arg in args)
            {
                switch (arg.Type)
                {
                    case AstType.Number:
                    case AstType.Time:
                    case AstType.Duration:
                        {
                            var value = arg.Evaluate(thread);
                            Console.WriteLine(value);
                            break;
                        }
                    case AstType.String:
                        {
                            var value = arg.Evaluate(thread);
                            Console.WriteLine($"\"{value}\"");
                            break;
                        }
                    case AstType.Object:
                        {
                            arg.Evaluate(thread);
                            Console.WriteLine(arg.ToString());
                            break;
                        }
                    default:
                        {
                            arg.Evaluate(thread);
                            Console.WriteLine(arg.Type);
                            break;
                        }
                }
            }
        }

        [Function(name: "add", description: "Display the arguments")]
        public static double Add(ScriptThread thread, List<AstNode> args)
        {
            var sum = 0D;
            foreach (var arg in args)
            {
                sum += arg.EvaluateDouble(thread);
            }

            return sum;
        }
    }
}
