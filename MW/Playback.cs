using NAudio.Wave;

namespace MW
{
    public static class Playback
    {
        private static WaveStream? fileReader;
        public static WaveOutEvent WaveOut { get; private set; } = new WaveOutEvent();

        [Command(name: "stop", description: "Stop playback")]
        public static void Stop()
        {
            EnsureStopped();
        }

        [Command(name: "play", arguments:"[<sample>|Ø]", description: "Play sample or resume current playback")]
        public static void Play(string sampleName)
        {
            if (fileReader == null)
            {
                Show.Error($"No file to play");
                return;
            }

            if (string.IsNullOrEmpty(sampleName))
            {
                if (WaveOut.PlaybackState == PlaybackState.Stopped)
                {
                    WaveOut.Play();
                }

                return;
            }

            EnsureStopped();

            try
            {
                var relativePath = Path.Combine(Env.DownloadFolderName, sampleName);
                var filePath = Path.Combine(Env.ProjectPath, relativePath);

                if (!File.Exists(filePath))
                {
                    Show.Error($"File not found: {relativePath}");
                    return;
                }

                if (!sampleName.EndsWith("wav"))
                {
                    Show.Error("Cannot play back non-wav file");
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
