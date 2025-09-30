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
        [Command(name:"close", description: "Close project")]
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
            Env.ChangesMade = false;

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

        [Command(name:"load", arguments:"<project_file>", description: "Load project")]
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
            //Project? newProject;

            var newProject = File.ReadAllLines(filePath);

            if (newProject != null)
            {
                WAEditor.LoadText(newProject);
                Env.ProjectPath = path;
                Env.IsProjectLoaded = true;
                Env.ChangesMade = false;

                var fullPath = Path.GetFullPath(filePath);
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

        [Command(name: "save", description: "Save project")]
        public static void Save()
        {
            if (Show.NoProjectLoaded())
            {
                return;
            }

            DoSave();

            Show.Hint($"Saved project file");
        }

        [Command(name: "list", description: "List audio files")]
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

        [Command(name: "touch", arguments:"<file_name>", description: "Create empty audio file")]
        public static void Touch(string fileName)
        {
            if (Show.NoProjectLoaded())
            {
                return;
            }

            Env.ChangesMade = true;
            
            Show.Hint("Touched");
        }

        [Command(name: "clear", description: "Clear cache")]
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

        private static List<string> GetAudioFiles()
        {
            if (Show.NoProjectLoaded(silent: true))
            {
                return [];
            }

            var path = Path.Combine(Env.ProjectPath, Env.DownloadFolderName);
            var files = Directory.EnumerateFiles(String.IsNullOrEmpty(path) ? "." : path);

            return files.Where(f => IsAudioFile(f)).Select(f => Path.GetFileName(f)).ToList();
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

        private static bool IsAudioFile(string fileName)
        {
            if (Path.GetExtension(fileName) == ".wav")
            {
                return true;
            }
            return false;
        }
    }
}
