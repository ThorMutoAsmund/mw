using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MW.Commands
{
    public static class YT
    {
        private static string YTToolname = "yt-dlp.exe";

        [Command(name: "yt", arguments: "<url>", description: "Download mp3 from YouTube")]
        public static void Download(string url)
        {
            // Debug url
            if (string.IsNullOrEmpty(url) && Env.IsDebug)
            {
                url = "https://www.youtube.com/watch?v=gdZLi9oWNZg";
            }

            // Check if tool exists
            var ytDlpPath = Path.Combine(Env.ToolsPath, YTToolname);
            if (!File.Exists(ytDlpPath))
            {
                Show.Error($"YT download tool not found at path: {ytDlpPath}");
                return;
            }

            // Check download folder
            var downloadPath = Path.Combine(Env.ProjectPath, Env.DownloadFolderName);
            if (!Directory.Exists(downloadPath))
            {
                Show.Error($"Download path not found");
                return;
            }

            // Get outputname
            var outputArgument = string.Empty;
            if (GetV(url, out var v))
            {
                outputArgument = $" -o {v}.wav";
            }

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = ytDlpPath,    // Replace with your program
                    Arguments = $"-f bestaudio{outputArgument} --extract-audio --audio-format wav \"{url}\"",
                    WorkingDirectory = downloadPath, 
                    CreateNoWindow = true,       // No console window (for console apps)
                    UseShellExecute = false,     // Needed to hide window for console apps
                    WindowStyle = ProcessWindowStyle.Hidden // Hide GUI apps
                },
                EnableRaisingEvents = true      // Required for Exited event
            };

            process.Exited += (sender, e) =>
            {
                if (process.ExitCode != 0)
                {
                    Show.Hint($"File download failed. Code {process.ExitCode}");
                }
                else
                {
                    if (!string.IsNullOrEmpty(outputArgument))
                    {
                        Show.Hint($"File {v}.wav download finished");
                    }
                    else
                    {
                        Show.Hint($"File download finished");
                    }
                }
            };

            process.Start();
            Show.Hint("Process started in the background...");
        }

        private static bool GetV(string url, out string v)
        {
            var uri = new Uri(url);

            var parameters = uri.Query.TrimStart('?')
                .Split('&', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Split('='))
                .ToDictionary(kv => kv[0], kv => kv[1]);

            if (parameters.ContainsKey("v"))
            {
                v = parameters["v"];
                return true;
            }

            v = string.Empty;
            return false;
        }
    }
}
