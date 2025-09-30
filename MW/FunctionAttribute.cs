using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MW
{
    using MW.Parsing;
    using System;

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class FunctionAttribute : Attribute
    {
        public bool IsCommandLine { get; } = false;
        public string Name { get; }
        public string Description { get; }
        public AstType AstType { get; }
        public bool ReturnsTypedValue { get; }

        public string Alt { get; }
        public string NameWithAlt => Name + (!string.IsNullOrEmpty(Alt) ? $" | {Alt}" : "");
        public string Arguments { get; }


        public FunctionAttribute(string name, bool isCommandLine = false, string alt = "", string arguments = "", string description = "", 
            AstType astType = AstType.Unset, bool returnsTypedValue = false)
        {
            this.IsCommandLine = isCommandLine;
            this.Name = name;
            this.Alt = alt;
            this.Arguments = arguments;
            this.Description = description;
            this.AstType = astType;
            this.ReturnsTypedValue = returnsTypedValue;
        }
    }
}
