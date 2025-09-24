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
        Unset, Number, String, Object, Generator, Time, Duration, RawSample
    }

    public abstract class TypedAstNode : AstNode
    {
        public virtual AstType Type { get; protected set; } = AstType.Unset;
    }
}
