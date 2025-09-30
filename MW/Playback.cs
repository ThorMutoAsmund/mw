using NAudio.Wave;

namespace MW
{
    public static class Playback
    {
        private static WaveStream? fileReader;
        public static WaveOutEvent WaveOut { get; private set; } = new WaveOutEvent();

        public static bool Toggle(string srcName)
        {
            if (WaveOut.PlaybackState == PlaybackState.Stopped)
            {
                Play(srcName);
                return true;
            }
            else
            {
                Stop();
                return false;
            }
        }

        [Command(name: "stop", description: "Stop playback")]
        public static void Stop()
        {
            EnsureStopped();
        }

        [Command(name: "play", arguments:"[<sample>|Ø]", description: "Play sample or resume current playback")]
        public static void Play(string srcName)
        {

            if (string.IsNullOrEmpty(srcName))
            {
                if (fileReader == null)
                {
                    Show.Error($"No file to play");
                    return;
                }

                if (WaveOut.PlaybackState == PlaybackState.Stopped)
                {
                    WaveOut.Play();
                }

                return;
            }

            EnsureStopped();

            try
            {
                if (!SrcExists(srcName, out var message, out var filePath))
                {
                    Show.Error(message);
                    return;
                }

                fileReader = new WaveFileReader(filePath);
                WaveOut.Init(fileReader);
                WaveOut.Play();
            }
            catch (Exception ex)
            {
                Show.Error($"Playback error: {ex.Message}");
            }
        }

        public static bool SrcExists(string srcName, out string message, out string filePath)
        {
            var relativePath = Path.Combine(Env.DownloadFolderName, srcName);
            filePath = Path.Combine(Env.ProjectPath, relativePath);

            if (!File.Exists(filePath))
            {
                message = $"File not found: {relativePath}";
                return false;
            }

            if (!srcName.EndsWith("wav"))
            {
                message = "Src not a wav file";
                return false;
            }

            message = string.Empty;
            return true;
        }

        public static void Reset()
        {
            if (fileReader != null)
            {
                fileReader.Dispose();
                fileReader = null;
            }
            //mp3Reader.Dispose();
            //FileWaveOut.Dispose();
        }

        private static void EnsureStopped()
        {
            if (WaveOut.PlaybackState != PlaybackState.Stopped)
            {
                WaveOut.Stop();
                //if (fileReader != null)
                //{
                //    fileReader.Dispose();
                //    fileReader = null;
                //}
                //mp3Reader.Dispose();
                //FileWaveOut.Dispose();
            }
        }
    }
}
