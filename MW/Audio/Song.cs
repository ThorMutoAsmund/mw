using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MW.Audio
{
    public class Song : SongElement
    {
        public static Song EmptySong = new();
        public List<Sample> Samples { get; private set; } = [];
        public WaveFormat Format { get; } = WaveFormat.CreateIeeeFloatWaveFormat(44100, 2); //new WaveFormat(44100, 32, 2);
        public WaveStream? WaveStream { get; private set; }
        public AudioSource? AudioSource { get; private set; }
        public bool IsWaveStreamCreated => this.WaveStream != null;

        public override string HashValue => this.AudioSource?.HashValue ?? Guid.Empty.ToString();
        public Song(AudioSource audioSource)
        {
            this.AudioSource = audioSource;                 
        }

        public Song()
        {
        }

        public static Song FromAudioSource(AudioSource audioSource)
        {
            return new Song(audioSource);
        }

        public WaveStream GetWaveStream()
        {
            if (this.IsWaveStreamCreated)
            {
                return this.WaveStream!;
            }

            if (this.AudioSource == null)
            {
                this.WaveStream = new WaveProviderToWaveStream(new SilenceProvider(Format));
                
                return this.WaveStream;
            }

            this.WaveStream = this.AudioSource.GetWaveStream(this.Format);

            return this.WaveStream;
        }

        public Sample FindSample(string existingFilePath)
        {
            var sample = this.Samples.FirstOrDefault(s => s.FilePath == existingFilePath);
            if (sample != null)
            {
                return sample;
            }

            sample = new Sample(existingFilePath);
            this.Samples.Add(sample);
            
            return sample;
        }

        public override string ToString() => nameof(Song);
    }
}
