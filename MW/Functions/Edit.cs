using Irony.Parsing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MW.Functions
{
    public static class Edit
    {
        [Function(name: "cut", description: "Cut a part of a sample")]
        public static void Cut(ParseTreeNodeList args)
        {
        }
    }
}
