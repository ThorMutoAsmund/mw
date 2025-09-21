using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MW
{
    using System;

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class CommandAttribute : Attribute
    {
        public string Name { get; }
        public string Alt { get; }
        public string NameWithAlt => Name + (!string.IsNullOrEmpty(Alt) ? $" | {Alt}" : "");
        public string Arguments { get; }
        public string Description { get; }

        public CommandAttribute(string name, string alt = "", string arguments = "", string description = "")
        {
            Name = name;
            Alt = alt;
            Arguments = arguments;
            Description = description;
        }
    }
}
