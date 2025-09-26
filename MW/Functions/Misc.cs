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
                        {
                            WAEditor.ShowInfo(value);
                            break;
                        }
                    case AstType.Time:
                        {
                            WAEditor.ShowInfo($"@{value}s");
                            break;
                        }
                    case AstType.Duration:
                        {
                            WAEditor.ShowInfo($"{value}s");
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
    }
}
