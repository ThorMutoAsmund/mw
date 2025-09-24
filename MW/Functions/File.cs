using Irony.Parsing;
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
        [Function(name: "raw", description: "Cut a part of a sample")]
        public static void Raw(ParseTreeNodeList args)
        {
        }

        [Function(name: "s", description: "Cut a part of a sample")]
        public static void Sample(ParseTreeNodeList args)
        {
        }
    }
}
