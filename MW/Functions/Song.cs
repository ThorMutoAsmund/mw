using Irony.Interpreter;
using Irony.Interpreter.Ast;
using MW.Helpers;
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
        [Function(name: "bpm", description: "Set project BPM")]
        public static void BPM(MethodContext context)
        {
            Func.ValidateArgCnt(nameof(BPM), context, numberOfArgs:1);

            var bpm = context.Args[0].EvaluateDouble(context.Thread);

            Func.ValidateRange(nameof(BPM), bpm, min: 20, max:1000);

            context.Settings[Constants.BMP] = bpm;
        }

        [Function(name: "seek", description: "Set seek time(s) in seconds")]
        public static void Seek(MethodContext context)
        {
            Func.ValidateArgCnt(nameof(Seek), context, minNumberOfArgs: 1);

            var seekTime = context.Args[0].EvaluateDouble(context.Thread);
            Func.ValidateRange(nameof(Seek), seekTime, min: 0, max: 120);
            context.Settings[Constants.SeekTime] = seekTime;

            if (context.Args.Count > 1)
            {
                var smallSeekTime = context.Args[1].EvaluateDouble(context.Thread);
                Func.ValidateRange(nameof(Seek), smallSeekTime, min: 0, max: 120);
                context.Settings[Constants.FineSeekTime] = smallSeekTime;
            }
        }

        [Function(name: "jump", description: "Set jump points")]
        public static void Jump(MethodContext context)
        {
            Func.ValidateArgCnt(nameof(Jump), context, minNumberOfArgs: 1);

            List<double> setPoints = [];
            for (int i = 0; i < context.Args.Count; ++i)
            {
                var setPoint = context.Args[i].EvaluateDouble(context.Thread);
                Func.ValidateRange(nameof(Jump), setPoint, min: 0);

                setPoints.Add(setPoint);
            }

            context.Settings[Constants.JumpPoints] = setPoints;
        }

        [Function(name: "time", returnsTypedValue: true, description: "Add a duration to a time")]
        public static Tuple<object, AstType> Time(MethodContext context)
        {
            Func.ValidateArgCnt(nameof(Time), context, numberOfArgs: 2);

            var time = context.Args[0].EvaluateDouble(context.Thread)
                + context.Args[1].EvaluateDouble(context.Thread);

            var returnType = context.Args[0].Type == AstType.Time ||
                context.Args[1].Type == AstType.Time ? 
                AstType.Time : AstType.Duration;

            return new Tuple<object, AstType>(time, returnType);
        }
    }
}
