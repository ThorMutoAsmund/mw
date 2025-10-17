using Irony.Parsing;
using MW.Audio;
using MW.Helpers;
using MW.Parsing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MW.Functions
{
    public static class Samples
    {
        [Function(name: "s", astType: AstType.Sample, description: "Load sample")]
        public static Sample Sample(MethodContext context)
        {
            Func.ValidateArgCnt(nameof(Samples), context, numberOfArgs: 1);

            var srcName = context.Args[0].EvaluateString(context.Thread) ?? string.Empty;

            if (!Project.SrcExists(srcName, out var message, out var filePath))
            {
                throw new RunException(message);
            }

            return Env.Song.FindSample(filePath);
        }
    }
}
