using Irony.Interpreter;
using Irony.Interpreter.Ast;
using MW.Parsing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MW.Helpers
{
    public static class Func
    {
        public static void ValidateArgCnt(string name, MethodContext context, int? numberOfArgs = null, int? minNumberOfArgs = null)
        {
            if (numberOfArgs.HasValue && context.Args.Count != numberOfArgs.Value)
            {
                throw new RunException($"{name} requires exactly {numberOfArgs.Value} argument(s)");
            }
            if (minNumberOfArgs.HasValue && context.Args.Count < minNumberOfArgs.Value)
            {
                throw new RunException($"{name} requires at least {minNumberOfArgs.Value} argument(s)");
            }
        }

        public static void ValidateRange(string name, double value, double? min = null, double? max = null)
        {
            if (min.HasValue && max.HasValue)
            {
                if (value < min.Value || value > max.Value)
                {
                    throw new RunException($"{name} requires the value {value} to be between {min.Value} and {max.Value}");
                }
            }
            if (min.HasValue)
            {
                if (value < min.Value)
                {
                    throw new RunException($"{name} requires the value {value} to be higher than {min.Value}");
                }
            }
            if (max.HasValue)
            {
                if (value > max.Value)
                {
                    throw new RunException($"{name} requires the value {value} to be lower than {max.Value}");
                }
            }
        }

        public static T ConvertAndValidate<T>(AstNode node, ScriptThread thread, string name = "Argument") where T : class
        {
            var result = node.Evaluate<T>(thread);
            if (result == null)
            {
                throw new RunException($"{name} must be of type {typeof(T).Name}");
            }

            return result;
        }
    }
}