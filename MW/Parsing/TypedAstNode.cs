using Irony.Interpreter.Ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MW.Parsing
{
    public enum AstType
    {
        Unset, Number, Text, Object, Time, Duration, CSObject
    }

    public abstract class TypedAstNode : AstNode
    {
        public virtual AstType Type { get; protected set; } = AstType.Unset;
        public string Error { get; set; } = string.Empty;
        public bool HasError => !string.IsNullOrEmpty(Error);   
        public void SetType(AstType newType)
        {
            if (this.Type == AstType.Unset)
            {
                this.Type = newType;
            }
        }
    }
}