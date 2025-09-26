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
    public static class File
    {
        [Function(name: "s", astType: AstType.RawSample, description: "Load sample")]
        public static string Sample(MethodContext context)
        {
            Func.ValidateArgs(nameof(Sample), context, 1);

            var srcName = context.Args[0].EvaluateString(context.Thread) ?? string.Empty;

            if (!Playback.SrcExists(srcName, out var message, out var filePath))
            {
                throw new RunException(message);
            }

            return srcName;
        }
    }
}
