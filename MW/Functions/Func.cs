using Irony.Interpreter;
using Irony.Interpreter.Ast;
using MW.Parsing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MW.Functions
{
    public static class Func
    {
        public static void ValidateArgs(string name, MethodContext context, int numberOfArgs)
        {
            if (context.Args.Count != numberOfArgs)
            {
                throw new RunException($"{name} requires exactly {numberOfArgs} argument(s)");
            }
        }

        public static void ValidateRange(string name, double value, double min, double max)
        {
            if (value < min || value > max)
            {
                throw new RunException($"{name} requires the value {value} to be between {min} and {max}");
            }
        }
    }
}