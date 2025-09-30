using Irony.Parsing;
using MW.Parsing;
using System.Text;

namespace MW
{
    internal class Program
    {
        static void Main(string[] args)
        {
            WAEditor.Recalc += lines =>
            {
                WAParser.Parse(lines);
                Project.DoSave();
            };

            // Read project file
            if (Env.IsDebug)
            {
                Project.TryLoadDefault();
            }

            // Center
            WindowHelper.CenterWindow();

            // Start editor
            WAEditor.Run();

            Show.Info("Goodbye!");
        }
    }
}
