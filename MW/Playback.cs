using MW.Parsing;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace MW
{
    public static class Playback
    {
        private static string currentSongHash = string.Empty;
        private static WaveStream? fileReader;
        public static WaveOutEvent WaveOut { get; private set; } = new WaveOutEvent();
        public static bool IsStopped => WaveOut.PlaybackState == PlaybackState.Stopped;

        private static double SeekTime;
        private static double FineSeekTime;
        private static List<double> JumpPoints = [];


        [Function(isCommandLine: true, name: "stop", description: "Stop playback")]
        public static void Stop()
        {
            EnsureStopped();
        }

        public static double? Seek(double mult)
        {
            if (!Env.IsProjectLoaded || IsStopped)
            {
                return null;
            }

            return UpdateCurrentTime(add: TimeSpan.FromSeconds(mult * SeekTime))?.TotalSeconds;
        }
        public static double? SeekFine(double mult)
        {
            if (!Env.IsProjectLoaded || IsStopped)
            {
                return null;
            }

            return UpdateCurrentTime(add: TimeSpan.FromSeconds(mult * FineSeekTime))?.TotalSeconds;
        }

        public static double? JumpTo(int index)
        {
            if (!Env.IsProjectLoaded || IsStopped)
            {
                return null;
            }

            if (index >= 0 && index < JumpPoints.Count)
            {
                var setPoint = JumpPoints[index];

                return UpdateCurrentTime(setTo: TimeSpan.FromSeconds(setPoint))?.TotalSeconds;
            }

            return null;
        }

        public static bool TogglePlaySong()
        {
            if (!Env.IsProjectLoaded)
            {
                EnsureStopped();
                return false;
            }

            // Toggle old song
            if (IsStopped)
            {
                // Check if new song should be generated
                EnsureSongGenerated();

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

                if (IsStopped)
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
            var deviceIndex = WaveOut.DeviceNumber;   // -1 means default
            var name = deviceIndex >= 0
                ? WinMMHelper.GetWaveOutName(deviceIndex)
                : new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia).FriendlyName;

            return name;
        }

        private static TimeSpan? UpdateCurrentTime(TimeSpan? setTo = null, TimeSpan? add = null)
        {
            if (fileReader != null)
            {
                if (setTo.HasValue)
                {
                    if (setTo.Value < fileReader.TotalTime && setTo.Value >= TimeSpan.Zero)
                    {
                        WaveOut.Pause();                      // or Stop(); either is fine
                        fileReader.CurrentTime = setTo.Value;
                        WaveOut.Play();

                        return setTo;
                    }
                }
                else if (add.HasValue)
                {
                    var newValue = fileReader.CurrentTime + add.Value;

                    if (newValue < TimeSpan.Zero)
                    {
                        newValue = TimeSpan.Zero;
                    }

                    if (newValue < fileReader.TotalTime)
                    {
                        WaveOut.Pause();                      // or Stop(); either is fine
                        fileReader.CurrentTime = newValue;
                        WaveOut.Play();

                        return newValue;
                    }

                }
            }
            return null;
        }

        private static void EnsureStopped()
        {
            if (!IsStopped)
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

        private static void EnsureSongGenerated()
        {
            // If a new song has been gnenerated
            if (Env.Song.HashValue != currentSongHash)
            {
                EnsureStopped();

                currentSongHash = Env.Song.HashValue;
                fileReader = Env.Song.GetWaveStream();

                WaveOut.Init(fileReader);

                // Read parse constants
                SeekTime = WAParser.Settings.ContainsKey(Constants.SeekTime) ? (double)WAParser.Settings[Constants.SeekTime] : Env.DefaultSeekTIme;
                FineSeekTime = WAParser.Settings.ContainsKey(Constants.FineSeekTime) ? (double)WAParser.Settings[Constants.FineSeekTime] : Env.DefaultFineSeekTime;
                JumpPoints = WAParser.Settings.ContainsKey(Constants.JumpPoints) ? WAParser.Settings[Constants.JumpPoints] as List<double> ?? [] : [];
            }
        }
    }
}
