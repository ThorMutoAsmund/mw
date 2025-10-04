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
        public WaveFormat Format { get; } = new WaveFormat(44100, 16, 2);

        private SilenceProvider Silence { get; init; }

        public Song()
        {
            this.Silence = new SilenceProvider(Format);
        }

        public static Song FromParseResult(object parseResult)
        {
            return EmptySong;
        }

        public WaveStream GetWaveStream()
        {
            return new WaveProviderToWaveStream(Silence);
        }

        public Sample GetOrCreateSample(string src)
        {
            var sample = this.Samples.FirstOrDefault(s => s.Src == src);
            if (sample != null)
            {
                return sample;
            }

            sample = new Sample(src);
            this.Samples.Add(sample);
            
            return sample;
        }

        public override string ToString() => nameof(Song);
    }
}
