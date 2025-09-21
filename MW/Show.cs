using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MW
{
    public static class Show
    {
        public static void Info(string message)
        {
            Console.WriteLine(message);
        }

        public static void Hint(string message)
        {
            if (Env.ShowHints)
            {
                Console.WriteLine(message);
            }
        }

        public static void Warning(string message)
        {
            Console.WriteLine($"[WARNING] {message}");
        }

        public static void Error(string message)
        {
            Console.WriteLine($"[ERROR] {message}");
        }

        public static bool Confirm(string message)
        {
            Console.Write(message + " (Y/N)");
            var answer = Console.ReadLine()?.ToLowerInvariant();
            
            return answer == "y";
        }

        public static bool NoProjectLoaded(bool silent = false)
        {
            if (!Env.Loaded)
            {
                if (!silent)
                {
                    Show.Error("No project loaded");
                }
                return true;
            }

            return false;
        }

        public static bool OkToDiscardChanges()
        {
            if (Env.Loaded && Env.ChangesMade)
            {
                var doQuit = Show.Confirm("Changes have been made. Are you sure you want to continue?");
                return doQuit;
}

            return true;
        }

    }
}
