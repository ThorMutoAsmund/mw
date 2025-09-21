using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MW
{
    public static class Engine
    {
        private static int iterations = 0;
        public static void Configure()
        {
            WAEditor.Recalc += lines =>
            {
                WAEditor.SetDebug($"Rendered {++iterations} times");
            };
        }
    }
}
