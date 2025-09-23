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
        public static void Show(ScriptThread thread, List<AstNode> args)
        {
            foreach (var arg in args)
            {
                var value = arg.Evaluate(thread);
                Console.WriteLine(value);
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
