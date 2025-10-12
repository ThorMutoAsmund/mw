using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MW.Audio
{
    public class Song : Container
    {
        public static Song EmptySong = new();
        public List<Sample> Samples { get; private set; } = [];
        public WaveFormat Format { get; } = new WaveFormat(44100, 32, 2);
        public WaveStream? WaveStream { get; private set; }
        public Sample? Output { get; private set; }
        public bool IsWaveStreamCreated => this.WaveStream != null;

        public Song(Sample output)
        {
            this.Output = output;                 
        }

        public Song()
        {
        }

        public static Song FromParseResult(object parseResult)
        {
            if (!(parseResult is Sample output))
            {
                return EmptySong;
            }

            return new Song(output);
        }

        public WaveStream GetWaveStream()
        {
            if (this.IsWaveStreamCreated)
            {
                return this.WaveStream!;
            }

            if (this.Output == null)
            {
                this.WaveStream = new WaveProviderToWaveStream(new SilenceProvider(Format));
                
                return this.WaveStream;
            }

            this.WaveStream = this.Output.GetWaveStream(this.Format);

            return this.WaveStream;
        }

        public Sample GetOrCreateSample(string existingFilePath)
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
