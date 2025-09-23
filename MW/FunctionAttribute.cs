using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MW
{
    using System;

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class FunctionAttribute : Attribute
    {
        public string Name { get; }
        public string Description { get; }

        public FunctionAttribute(string name, string description = "")
        {
            Name = name;
            Description = description;
        }
    }
}
