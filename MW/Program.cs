using Irony.Parsing;
using MW.Parsing;
using System.Text;

namespace MW
{
    internal class Program
    {

        static void Main(string[] args)
        {
            //ExprGrammar.Test();
            WAGrammar.Test();

            return;

            Engine.Configure();

            Startup.Run();
            WAEditor.Run();

            Show.Info("Goodbye!");
        }
    }
}
