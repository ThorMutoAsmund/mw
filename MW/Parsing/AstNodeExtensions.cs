using Irony.Interpreter;
using Irony.Interpreter.Ast;
using Irony.Parsing;

namespace MW.Parsing
{
    public static class AstNodeExtensions
    {
        //public static object Evaluate(this ParseTreeNode node, ScriptThread thread)
        //{
        //    return ((AstNode)node.AstNode).Evaluate(thread);
        //}

        public static double EvaluateDouble(this AstNode node, ScriptThread thread)
        {
            return Convert.ToDouble(node.Evaluate(thread));
        }

        public static string? EvaluateString(this AstNode node, ScriptThread thread)
        {
            return Convert.ToString(node.Evaluate(thread));
        }

        public static T? Evaluate<T>(this ParseTreeNode node, ScriptThread thread) where T: class
        {
            return (node.Evaluate(thread)) as T;
        }
    }
}
