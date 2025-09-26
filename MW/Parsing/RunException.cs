using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MW.Parsing
{
    public class RunException : Exception
    {
        public RunException()
        {
        }

        public RunException(string? message) : base(message)
        {
        }
    }
}
