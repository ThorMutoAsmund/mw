using MW.Parsing;
using NAudio.Wave;

namespace MW
{
    public static class Playback
    {
        private static Guid currentSongHash = Guid.Empty;
        private static WaveStream? fileReader;
        public static WaveOutEvent WaveOut { get; private set; } = new WaveOutEvent();

        //public static bool Toggle(string srcName)
        //{
        //    if (WaveOut.PlaybackState == PlaybackState.Stopped)
        //    {
        //        PlaySample(srcName);
        //        return true;
        //    }
        //    else
        //    {
        //        Stop();
        //        return false;
        //    }
        //}

        [Function(isCommandLine: true, name: "stop", description: "Stop playback")]
        public static void Stop()
        {
            EnsureStopped();
        }

        public static bool PlaySong()
        {
            if (!Env.IsProjectLoaded)
            {
                EnsureStopped();
                return false;
            }

            // If a new song has been gnenerated
            if (Env.Song.Hash != currentSongHash)
            {
                EnsureStopped();

                currentSongHash = Env.Song.Hash;
                fileReader = Env.Song.GetWaveStream();

                WaveOut.Init(fileReader);
                WaveOut.Play();
                return true;
            }

            if (WaveOut.PlaybackState == PlaybackState.Stopped)
            {
                WaveOut.Play();
                return true;
            }
            else
            {
                EnsureStopped();
                return false;
            }
        }

        [Function(isCommandLine: true, name: "play", arguments:"[<sample>|Ø]", description: "Play sample or resume current playback")]
        public static void PlaySample(string srcName)
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
                if (!Project.SrcExists(srcName, out var message, out var filePath))
                {
                    Show.Error(message);
                    return;
                }

                fileReader = new AudioFileReader(filePath);
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
