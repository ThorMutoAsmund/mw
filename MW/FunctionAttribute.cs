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
        public string Name { get; }
        public string Description { get; }
        public AstType AstType { get; }
        public bool ReturnsTypedValue { get; }

        public FunctionAttribute(string name, string description = "", 
            AstType astType = AstType.Unset, bool returnsTypedValue = false)
        {
            this.Name = name;
            this.Description = description;
            this.AstType = astType;
            this.ReturnsTypedValue = returnsTypedValue;
        }
    }
}
