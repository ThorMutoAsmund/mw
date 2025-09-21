using Irony.Interpreter;
using Irony.Interpreter.Ast;
using Irony.Parsing;

namespace MW.Parsing
{
    public static class ParseTreeNodeExtensions
    {
        public static object Evaluate(this ParseTreeNode node, ScriptThread thread)
        {
            return ((AstNode)node.AstNode).Evaluate(thread);
        }

        public static double EvaluateDouble(this ParseTreeNode node, ScriptThread thread)
        {
            return Convert.ToDouble(((AstNode)node.AstNode).Evaluate(thread));
        }

        public static string? EvaluateString(this ParseTreeNode node, ScriptThread thread)
        {
            return Convert.ToString(((AstNode)node.AstNode).Evaluate(thread));
        }

        public static T? Evaluate<T>(this ParseTreeNode node, ScriptThread thread) where T: class
        {
            return ((AstNode)node.AstNode)?.Evaluate(thread) as T;
        }
    }
}
