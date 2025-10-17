using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MW
{
    public static class Project
    {
        public static string[] SupportedFormats { get; private set; } = ["wav", "mp3"];

        [Function(isCommandLine: true, name:"close", description: "Close project")]
        public static void Close()
        {
            if (Show.NoProjectLoaded())
            {
                return;
            }

            if (!Show.OkToDiscardChanges())
            {
                return;
            }

            Env.IsProjectLoaded = false;
            Env.AreChangesMade = false;

            Show.Hint("Closed project");
        }

        public static void TryLoadDefault()
        {
            var filePath = Path.Combine(Env.ProjectPath, Env.DefaultProjectNameAndExtension);

            if (File.Exists(filePath))
            {
                Load(Env.ProjectPath);
            }
        }

        [Function(isCommandLine: true, name:"load", arguments:"<project_file>", description: "Load project")]
        public static bool Load(string path)
        {
            var filePath = Path.Combine(path, Env.DefaultProjectNameAndExtension);

            if (!Show.OkToDiscardChanges())
            {
                return false;
            }

            if (!File.Exists(filePath))
            {
                Show.Error($"Project file does not exist: {filePath}");

                return false;
            }

            // Load project file
            var newProject = File.ReadAllLines(filePath);

            if (newProject != null)
            {
                WAEditor.LoadText(newProject);
                Env.ProjectPath = path;
                Env.IsProjectLoaded = true;
                Env.AreChangesMade = false;

                var fullPath = Path.GetFullPath(filePath);

                ClearCache();

                Show.Hint($"Loaded project file: {fullPath}");

                return true;
            }

            return false;
        }

        public static void DoSave()
        {
            if (Env.IsProjectLoaded)
            {
                var filePath = Path.Combine(Env.ProjectPath, Env.DefaultProjectNameAndExtension);

                EnsurePaths(Env.ProjectPath);

                // Save project file
                File.WriteAllLines(filePath, WAEditor.Lines);
            }
        }

        [Function(isCommandLine: true, name: "save", description: "Save project")]
        public static void Save()
        {
            if (Show.NoProjectLoaded())
            {
                return;
            }

            DoSave();

            Show.Hint($"Saved project file");
        }

        [Function(isCommandLine: true, name: "list", alt: "l", description: "List audio files")]
        public static void List()
        {
            if (Show.NoProjectLoaded())
            {
                return;
            }

            var files = GetAudioFiles();

            foreach (var file in files)
            {
                Show.Info(file);
            }
        }

        [Function(isCommandLine: true, name: "touch", arguments:"<file_name>", description: "Create empty audio file")]
        public static void Touch(string fileName)
        {
            if (Show.NoProjectLoaded())
            {
                return;
            }

            Env.AreChangesMade = true;
            
            Show.Hint("Touched");
        }

        [Function(isCommandLine: true, name: "clear", description: "Clear cache")]
        public static void ClearCache()
        {
            if (Show.NoProjectLoaded())
            {
                return;
            }

            var cachePath = Path.Combine(Env.ProjectPath, Env.CacheFolderName);
            if (Directory.Exists(cachePath))
            {
                foreach (string file in Directory.GetFiles(cachePath))
                {
                    File.Delete(file);
                }
                Show.Hint("Cache cleared");
            }
        }

        public static string[] GetAudioFiles(bool onlyFileName = true)
        {
            if (!Env.IsProjectLoaded)
            {
                return [];
            }

            var downloadFolderPath = Path.Combine(Env.ProjectPath, Env.DownloadFolderName);
            var list = Directory.EnumerateFiles(downloadFolderPath, "*", SearchOption.TopDirectoryOnly)
                .Where(p => IsAudioFile(p));

            if (onlyFileName)
            {
                list = list.Select(p => Path.GetFileName(p));
            }

            return list.ToArray();
        }

        private static void EnsurePaths(string path)
        {
            if (Show.NoProjectLoaded(silent: true))
            {
                return;
            }

            // Ensure path
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            // Ensure sub directories
            var downloadPath = Path.Combine(path, Env.DownloadFolderName);
            if (!Directory.Exists(downloadPath))
            {
                Directory.CreateDirectory(downloadPath);
            }
            var cachePath = Path.Combine(path, Env.CacheFolderName);
            if (!Directory.Exists(cachePath))
            {
                Directory.CreateDirectory(cachePath);
            }
            var pluginsPath = Path.Combine(path, Env.PluginsFolderName);
            if (!Directory.Exists(pluginsPath))
            {
                Directory.CreateDirectory(pluginsPath);
            }
        }

        public static bool SrcExists(string partialSrcName, out string message, out string filePath)
        {
            filePath = string.Empty;
            var downloadFolder = Path.Combine(Env.ProjectPath, Env.DownloadFolderName);

            var files = Directory.GetFiles(downloadFolder);
            var fileName = files.FirstOrDefault(f => Path.GetFileName(f).StartsWith(partialSrcName));

            if (fileName is null)
            {
                message = $"No file matches '{partialSrcName}'";
                return false;
            }

            filePath = fileName;

            if (!Project.IsAudioFile(filePath))
            {
                message = $"'{Path.GetFileName(fileName)}' not a supported audio file";
                return false;
            }

            message = string.Empty;
            return true;
        }

        public static bool SrcExistsExact(string srcName, out string message, out string filePath)
        {
            var downloadFolder = Path.Combine(Env.ProjectPath, Env.DownloadFolderName);
            filePath = Path.Combine(downloadFolder, srcName);

            var s = 0;
            while (!File.Exists(filePath))
            {
                if (s >= SupportedFormats.Length)
                {
                    message = $"File not found: {srcName}";
                    return false;
                }
                filePath = Path.Combine(downloadFolder, $"{srcName}.{SupportedFormats[s]}");
                s++;
            }

            if (!Project.IsAudioFile(filePath))
            {
                message = "Src not a supported audio file";
                return false;
            }

            message = string.Empty;
            return true;
        }

        public static bool IsAudioFile(string fileNameOrPath)
        {
            var ext = Path.GetExtension(fileNameOrPath).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext))
            {
                return false;
            }

            return SupportedFormats.Contains(ext.Substring(1));
        }

        //public static bool IsWavFile(string fileNameOrPath)
        //{
        //    var ext = Path.GetExtension(fileNameOrPath).ToLowerInvariant();
        //    if (string.IsNullOrEmpty(ext))
        //    {
        //        return false;
        //    }

        //    return ext == ".wav";
        //}
    }
}
