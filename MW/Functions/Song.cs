using Irony.Interpreter;
using Irony.Interpreter.Ast;
using MW.Parsing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MW.Functions
{
    public static class Song
    {
        [Function(name: "bpm", description: "Cut a part of a sample")]
        public static void BPM(MethodContext context)
        {
            Func.ValidateArgs(nameof(BPM), context, 1);

            var bpm = context.Args[0].EvaluateDouble(context.Thread);

            Func.ValidateRange(nameof(BPM), bpm, 20, 1000);

            context.Settings[Constants.BMP] = bpm;
        }

        [Function(name: "time", returnsTypedValue: true, description: "Add a duration to a time")]
        public static Tuple<object,AstType> Time(MethodContext context)
        {
            Func.ValidateArgs(nameof(Time), context, 2);

            var time = context.Args[0].EvaluateDouble(context.Thread)
                + context.Args[1].EvaluateDouble(context.Thread);

            var returnType = context.Args[0].Type == AstType.Time ||
                context.Args[1].Type == AstType.Time ? 
                AstType.Time : AstType.Duration;

            return new Tuple<object, AstType>(time, returnType);
        }
    }
}
