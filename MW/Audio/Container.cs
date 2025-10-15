using MW.Helpers;
using MW.Parsing;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MW.Audio
{
    public class Container : AudioSource
    {
        public List<Instance> Instances { get; private set; } = [];
        public string WaveFilePath { get; protected set; } = string.Empty;
        public bool IsWaveFileGenerated => !string.IsNullOrEmpty(this.WaveFilePath);

        public void Add(SongElement source, double offset = 0D)
        {
            switch (source)
            {
                case AudioSource audioSource:
                    {
                        if (audioSource == this)
                        {
                            throw new RunException($"Cannot add a container to itself");
                        }
                        this.Instances.Add(Instance.CreateFrom(audioSource, offset));
                        break;
                    }
                default:
                    {
                        throw new RunException($"Arguments must be of type {nameof(AudioSource)}");
                    }
            }
        }

        public override WaveStream GetWaveStream(WaveFormat waveFormat)
        {
            EnsureWaveFileGenerated(waveFormat);

            return new WaveFileReader(this.WaveFilePath);
        }

        public override string ToString() => $"{nameof(Container)} with {this.Instances.Count} element(s)";

        private void EnsureWaveFileGenerated(WaveFormat targetFormat)
        {
            if (!this.IsWaveFileGenerated)
            {
                if (CreateWav(targetFormat, out var wavPath))
                {
                    this.WaveFilePath = wavPath;
                }
            }
        }

        private bool CreateWav(WaveFormat targetFormat, out string outputWavPath)
        {
            var outputFileName = Guid.NewGuid().ToString("N");
            var cachePath = Path.Combine(Env.ProjectPath, Env.CacheFolderName);

            outputWavPath = Path.Combine(cachePath, $"{outputFileName}.wav");

            FloatBuffer? buffer = null;

            foreach (var instance in this.Instances)
            {
                if (instance.SongElement is Sample sample)
                {
                    var data = sample.GetData(targetFormat);
                    var offsetInSamples = (int)(instance.Offset * targetFormat.SampleRate) * 2;
                    var dataLengthInSamples = data.Length;

                    if (buffer is null)
                    {
                        buffer = new(offsetInSamples + data.Length);
                        buffer.AppendZeros(offsetInSamples);
                        buffer.Append(data);
                    }
                    else
                    {
                        buffer.Add(offsetInSamples, data);
                    }
                }
            }

            if (buffer == null)
            {
                return false;
            }

            SaveFloatWav(outputWavPath, buffer.ToArray(), targetFormat);              // writes WAV in target format
            return true;
        }

        private void SaveFloatWav(string path, float[] samples, WaveFormat targetFormat)
        {
            using var writer = new WaveFileWriter(path, targetFormat);
            writer.WriteSamples(samples, 0, samples.Length); // interleaved L,R,L,R,...
        }
    }
}
