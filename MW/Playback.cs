using MW.Parsing;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace MW
{
    public static class Playback
    {
        private static Guid currentSongHash = Guid.Empty;
        private static WaveStream? fileReader;
        public static WaveOutEvent WaveOut { get; private set; } = new WaveOutEvent();

        [Function(isCommandLine: true, name: "stop", description: "Stop playback")]
        public static void Stop()
        {
            EnsureStopped();
        }

        public static int? JumpTo(int index)
        {
            if (!Env.IsProjectLoaded)
            {
                EnsureStopped();
                return null;
            }

            if (WAParser.Settings.ContainsKey(Constants.JumpPoints))
            {
                var setPoints = WAParser.Settings[Constants.JumpPoints] as List<double>;
                if (setPoints != null && index >= 0 && index < setPoints.Count)
                {
                    var setPoint = setPoints[index];

                    EnsureSongGenerated();

                    if (fileReader != null)
                    {
                        var t = TimeSpan.FromSeconds(setPoint);

                        if (t < fileReader.TotalTime)
                        {
                            WaveOut.Pause();                      // or Stop(); either is fine
                            fileReader.CurrentTime = t;
                            WaveOut.Play();

                            return index;
                        }
                    }
                }
            }

            return null;
        }

        public static bool PlaySong()
        {
            if (!Env.IsProjectLoaded)
            {
                EnsureStopped();
                return false;
            }

            // Check if new song should be generated
            var newSongGenerated = EnsureSongGenerated();
            if (newSongGenerated)
            {
                return true;
            }

            // Toggle old song
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

        public static string GetDeviceFriendlyName()
        {
            // usage (for a WaveOut/WaveOutEvent you created):
            int deviceIndex = WaveOut.DeviceNumber;   // -1 means default
            string name = deviceIndex >= 0
                ? WinMMHelper.GetWaveOutName(deviceIndex)
                : new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia).FriendlyName;

            return name;
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

        private static bool EnsureSongGenerated()
        {
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
            
            return false;
        }
    }
}
