using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MW
{
    public static class Env
    {
        public static bool IsDebug = true;
        public static bool ShowHints = true;

        public static string ApplicationName = $"ModWaves";
        public static string ApplicationNameAndVersion = $"{ApplicationName} {Assembly.GetExecutingAssembly().GetName().Version} (c) Thor Muto Asmund";
        public static string DefaultProjectNameAndExtension = "Project.mw.txt";
        public static string DownloadFolderName = "download";
        public static string CacheFolderName = "cache";
        public static string PluginsFolderName = "plugins";
        public static string ToolsPath = "../../../tools/";

        public static bool IsProjectLoaded = false;
        public static bool ChangesMade = false;
        public static string ProjectPath = IsDebug ? "../../../projects/default" : string.Empty;
        //public static Project Project { get; set; } = new Project();


        [Command(name: "hints", arguments:"[on|off|Ø]", description: "Toggle hints")]
        public static void SetHints(string value)
        {
            switch (value)
            {
                case "on": ShowHints = true; break;
                case "off": ShowHints = false; break;
                default: ShowHints = !ShowHints; break;
            }

            Show.Hint($"Hints {(ShowHints?"on":"off")}");
        }
    }
}
