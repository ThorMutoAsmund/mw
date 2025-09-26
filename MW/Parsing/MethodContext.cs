using Irony.Interpreter;
using MW.Parsing.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MW.Parsing
{
    public class MethodContext
    {
        public FuncNode Func { get; init; }
        public ScriptThread Thread { get; init; }
        public List<ArgNode> Args => this.Func.Args;
        public IDictionary<string, object> Globals => Thread.App.Globals;
        public IDictionary<string, object> Settings => (IDictionary<string, object>)Globals["settings"];
        public IDictionary<string, (object, AstType)> Vars => (IDictionary<string, (object, AstType)>)Globals["vars"];
        public MethodContext(FuncNode func, ScriptThread thread)
        {
            this.Func = func;
            this.Thread = thread;
        }
    }
}
