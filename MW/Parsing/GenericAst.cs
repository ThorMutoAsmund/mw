using Irony.Interpreter.Ast;
using Irony.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MW.Parsing
{
    public class NumberLiteral<T> : Irony.Parsing.NumberLiteral where T: AstNode
    {
        public NumberLiteral(string name) : base(name)
        {
            AstConfig.NodeType = typeof(T);
        }

        public NumberLiteral(string name, Irony.Parsing.NumberOptions options) : base(name, options)
        {
            AstConfig.NodeType = typeof(T);
        }
    }

    public class IdentifierTerminal<T> : Irony.Parsing.IdentifierTerminal where T : AstNode
    {
        public IdentifierTerminal(string name) : base(name)
        {
            AstConfig.NodeType = typeof(T);
        }

        public IdentifierTerminal(string name, Irony.Parsing.IdOptions options) : base(name, options)
        {
            AstConfig.NodeType = typeof(T);
        }
    }

    public class StringLiteral<T> : Irony.Parsing.StringLiteral where T : AstNode
    {
        public StringLiteral(string name) : base(name)
        {
            AstConfig.NodeType = typeof(T);
        }

        public StringLiteral(string name, string startEndSymbol) : base(name, startEndSymbol)
        {
            AstConfig.NodeType = typeof(T);
        }

        public StringLiteral(string name, string startEndSymbol, StringOptions options) : base(name, startEndSymbol, options)
        {
            AstConfig.NodeType = typeof(T);
        }
    }

    public class NonTerminal<T> : Irony.Parsing.NonTerminal where T : AstNode
    {
        public NonTerminal(string name) : base(name)
        {
            AstConfig.NodeType = typeof(T);
        }
    }

    public class TransientNonTerminal : Irony.Parsing.NonTerminal
    {
        public TransientNonTerminal(string name) : base(name)
        {
            //this.Grammar.MarkTransient(this);
            //AstConfig.NodeCreator = (ctx, n) =>
            //    n.AstNode = n.ChildNodes.Count == 1 ? n.ChildNodes[0].AstNode :
            //    n.ChildNodes[1].AstNode;

        }

        public override void Init(GrammarData grammarData)
        {
            base.Init(grammarData);
            grammarData.Grammar.MarkTransient(this);
        }
    }
}
