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
        public static void ShowInfo(MethodContext context)
        {
            foreach (var arg in context.Args)
            {
                var value = arg.Evaluate(context.Thread);

                switch (arg.Type)
                {
                    case AstType.Number:
                    case AstType.Time:
                    case AstType.Duration:
                        {
                            WAEditor.ShowInfo(value);
                            break;
                        }
                    case AstType.Text:
                        {
                            WAEditor.ShowInfo($"\"{value}\"");
                            break;
                        }
                    case AstType.Object:
                        {
                            WAEditor.ShowInfo(arg.Value.ToString());
                            break;
                        }
                    default:
                        {
                            WAEditor.ShowInfo(arg.Type);
                            break;
                        }
                }
            }
        }

        [Function(name: "add", description: "Display the arguments")]
        public static double Add(MethodContext context)
        {
            var sum = 0D;
            foreach (var arg in context.Args)
            {
                sum += arg.EvaluateDouble(context.Thread);
            }

            return sum;
        }
    }
}
