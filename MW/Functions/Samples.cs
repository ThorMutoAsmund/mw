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
        [Function(name: "s", astType: AstType.CSObject, description: "Load sample")]
        public static Sample Sample(MethodContext context)
        {
            Func.ValidateArgs(nameof(Samples), context, 1);

            var srcName = context.Args[0].EvaluateString(context.Thread) ?? string.Empty;

            if (!Project.SrcExists(srcName, out var message, out var filePath))
            {
                throw new RunException(message);
            }

            return Env.Song.GetOrCreateSample(filePath);
        }
    }
}
