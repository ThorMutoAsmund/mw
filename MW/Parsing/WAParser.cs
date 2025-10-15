using Irony.Interpreter;
//using Irony.Interpreter.Ast;
using Irony.Parsing;
using MW.Audio;
using MW.Parsing.Nodes;
using System;
using System.Xml.Linq;

namespace MW.Parsing
{
    public sealed class WAParser : Grammar
    {
        public const string PassOperator = "pass";
        public const string VariablePrefix = "$";
        public const string AssignmentOperator = ":";
        public const string ArgSeparator = ",";
        public const string StartObject = "{";
        public const string EndObject = "}";
        public const string ArgIdSuffix = ":";
        public const string BeatSuffix = "b";
        public const string SecondsSuffix = "s";
        public const string TimePrefix = "@";
        public const string PushOperator = ">";
        public const string PopOperator = "<";

        public const string PlusOperator = "+";
        public const string MinusOperator = "-";
        public const string MultiplicationOperator = "*";
        public const string DivisionOperator = "/";
        public const string StartParenthesis = "(";
        public const string EndParenthesis = ")";


        public static List<string> ParseErrors { get; private set; } = [];
        public static Dictionary<string, object> Settings { get; private set; } = [];
        public static Stack<AudioSource> AudioSourceStack { get; private set; } = new();
        public static AudioSource? Output { get; private set; } = null;
        public static AudioSource? CurrentAudioSource { get; private set; } = null;
        public static AudioSource? Head { get; private set; } = null;
        public static ParseTree? Tree { get; private set; } = null;
        public static string? Source { get; private set; } = null;
        public WAParser()
        {
            // Terminals
            NumberLiteral<NumberNode> numberTerm = new("number", NumberOptions.AllowLetterAfter);
            IdentifierTerminal<IdentNode> varIdentTerm = new("variableName");
            IdentifierTerminal<IdentNode> funcIdentTerm = new("name");
            IdentifierTerminal<IdentNode> argIdentTerm = new("id");
            StringLiteral<TextNode> stringTerm = new("string", "\"", StringOptions.None);

            // Non-terminals
            NonTerminal<LinesNode> lines = new("Lines");
            TransientNonTerminal line = new("Line");
            NonTerminal<AssignmentNode> assignment = new("Assign");
            NonTerminal<FuncNode> func = new("Func");
            NonTerminal<ObjectNode> obj = new("Object");
            TransientNonTerminal expr = new("Expr");
            NonTerminal<ArgNode> arg = new("Arg");
            NonTerminal<ArgsNode> args = new("Args");
            NonTerminal<BeatNode> beat = new("Beat");
            NonTerminal<SecondsNode> seconds = new("Seconds");
            NonTerminal<TimeNode> time = new("Time");
            NonTerminal<VariableNode> variable = new("Variable");

            NonTerminal<BinaryExprNode> nexpr = new("NExpr");
            NonTerminal<BinaryExprNode> term = new("Term");
            TransientNonTerminal factor = new("Factor");

            NonTerminal<PopNode> pop = new("Pop");
            NonTerminal<PushNode> push = new("Push");

            // EBNF-ish rules
            line.Rule = assignment | expr | pop | push;
            assignment.Rule = variable + AssignmentOperator + expr;
            expr.Rule = func | nexpr | stringTerm | obj;
            pop.Rule = PopOperator;
            push.Rule = PushOperator;
            variable.Rule = VariablePrefix + varIdentTerm;
            func.Rule = funcIdentTerm + StartParenthesis + args + EndParenthesis;
            nexpr.Rule = nexpr + PlusOperator + term | nexpr + MinusOperator + term | term;
            stringTerm.AddStartEnd("'", StringOptions.None);
            obj.Rule = StartObject + args + EndObject;
            arg.Rule = ArgIdSuffix + argIdentTerm + expr | expr;
            term.Rule = term + MultiplicationOperator + factor | term + DivisionOperator + factor | factor;
            factor.Rule = beat | seconds | time | numberTerm | variable | StartParenthesis + nexpr + EndParenthesis;
            beat.Rule = numberTerm + BeatSuffix;
            seconds.Rule = numberTerm + SecondsSuffix;
            time.Rule = TimePrefix + beat | TimePrefix + seconds;

            MakePlusRule(lines, line);
            MakeStarRule(args, ToTerm(ArgSeparator), arg);
            
            // Punctuation and precedence
            MarkPunctuation(StartObject, EndObject, StartParenthesis, EndParenthesis, ArgSeparator, VariablePrefix, 
                AssignmentOperator, BeatSuffix, SecondsSuffix, TimePrefix);
            //RegisterOperators(1, AssignmentOperator);
            //RegisterOperators(3, MultiplicationOperator, DivisionOperator);
            //RegisterOperators(2, PlusOperator, MinusOperator);

            // // line comments (handle \n and \r\n; EOF is handled automatically)
            CommentTerminal lineComment = new("LineComment", "//", "\n", "\r\n");

            // /* block comments */
            CommentTerminal blockComment = new("BlockComment", "/*", "*/");

            // Optional: ensure comments win if you also tokenize '/'
            lineComment.Priority = TerminalPriority.High;
            blockComment.Priority = TerminalPriority.High;

            // Tell Irony to ignore them (they won't appear as tokens/AST nodes)
            NonGrammarTerminals.Add(lineComment);
            NonGrammarTerminals.Add(blockComment);

            this.Root = lines;
            this.LanguageFlags = LanguageFlags.CreateAst;
        }

        public static void Parse(List<string>? input = null)
        {
            ParseErrors.Clear();    
            Output = null;
            Head = null;
            CurrentAudioSource = null;

            if (input == null)
            {
                Source = """
                // Comment before   
                bpm(4000)
                bpm(20,30)
                """;
            }
            else
            {
                Source = string.Join(Environment.NewLine, input);
            }

            LanguageData lang = new(new WAParser());
            Parser parser = new(lang);
            Tree = parser.Parse(Source);

            if (Tree.HasErrors())
            {
                foreach (var e in Tree.ParserMessages)
                {
                    ParseErrors.Add(e.Message);
                }

                return;
            }

            // Walk the parse tree
            // Print(tree.Root, 0);
            // Console.WriteLine(input);

            var runtime = new LanguageRuntime(lang);
            var app = new ScriptApp(runtime);
            var thread = new ScriptThread(app);

            // optional: pass variables/context to nodes via Globals
            Settings.Clear();
            AudioSourceStack.Clear();
            thread.App.Globals["vars"] = new Dictionary<string, (object, AstType)>();
            thread.App.Globals["settings"] = Settings;

            // Evaluate
            Tree.Root.Evaluate(thread);

            // If output not set from code, use the result from parsing the lins
            if (Output == null)
            {
                Output = CurrentAudioSource;
            }

            Env.Song = Output != null ? Song.FromAudioSource(Output) : Song.EmptySong;
        }

        public static void SetOutput(AudioSource output)
        {
            Output = output;
        }

        public static void SetCurrentAudioSource(AudioSource output)
        {
            CurrentAudioSource = output;
        }

        public static void Push()
        {
            if (CurrentAudioSource == null)
            {
                throw new RunException("No current audio source to push");
            }
            AudioSourceStack.Push(CurrentAudioSource);
            Head = CurrentAudioSource;
        }

        public static void Pop()
        {
            if (AudioSourceStack.Count == 0)
            {
                throw new RunException("Stack underflow");
            }

            CurrentAudioSource = AudioSourceStack.Pop();
            Head = AudioSourceStack.Count == 0 ? null : AudioSourceStack.Peek();
        }

        [Function(isCommandLine: true, name: "tree", description: "Shows the last parse tree")]
        public static void PrintTree()
        {
            if (Tree != null)
            {
                var node = Tree.Root;
                Print(node, 0);
            }
        }

        [Function(isCommandLine: true, name: "errors",alt: "e", description: "Shows the last parse errors")]
        public static void PrintErrors()
        {
            if (ParseErrors.Count > 0)
            {
                foreach (var e in ParseErrors)
                {
                    Console.WriteLine(e);
                }
                return;
            }

            var errorsFound = false;

            if (Tree != null && Source != null)
            {
                var linesNode = Tree.Root.AstNode as LinesNode;
                if (linesNode != null)
                {
                    foreach (var line in linesNode.ChildNodes)
                    {
                        if (line is TypedAstNode typedAstNode && 
                            typedAstNode.HasError)
                        {
                            errorsFound = true;

                            int lineNo = line.Span.Location.Line;    // 0-based
                            int colNo = line.Span.Location.Column;
                            
                            var start = line.Span.Location.Position; // absolute char index
                            var len = line.Span.Length;
                            var lineText = Source.Substring(start, len);

                            Console.WriteLine(typedAstNode.Error);
                            Console.WriteLine($"  in line {lineNo}: {lineText}");
                        }
                    }
                }
            }

            if (!errorsFound)
            {
                Console.WriteLine("No errors");
            }
        }

        [Function(isCommandLine: true, name: "result", alt: "r", description: "Shows the result of last parse")]
        public static void PrintResult()
        {
            if (Output == null)
            {
                Console.WriteLine("null");
                return;
            }

            Console.WriteLine(Output);
        }

        private static void Print(ParseTreeNode node, int indent)
        {
            Console.WriteLine(new string(' ', indent) + node);

            foreach (var ch in node.ChildNodes)
            {
                Print(ch, indent + 2);
            }
        }
    }
}
