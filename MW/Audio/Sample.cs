using MW.Parsing;
using NAudio.Wave;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MW.Audio
{
    public class Sample : AudioSource
    {
        public string FilePath { get; protected set; } = string.Empty;
        public string FileName { get; protected set; } = string.Empty;
        public string WaveFilePath { get; protected set; } = string.Empty;
        public bool IsWaveFileGenerated => !string.IsNullOrEmpty(this.WaveFilePath);
        public bool IsDataReady => this.Data != null;

        public float[]? Data { get; protected set; } = null;

        public Sample(string existingFilePath)
        {
            this.FilePath = existingFilePath;
            this.FileName = Path.GetFileName(existingFilePath);

            if (Project.IsWavFile(this.FilePath))
            {
                this.WaveFilePath = this.FilePath;
            }
        }


        public override WaveStream GetWaveStream(WaveFormat waveFormat)
        {
            EnsureWaveFileGenerated(waveFormat);

            return new WaveFileReader(this.WaveFilePath);
        }

        public float[] GetData(WaveFormat targetFormat)
        {
            EnsureDataReady(targetFormat);

            if (!this.IsDataReady)
            {
                return Array.Empty<float>();
            }

            return this.Data!;
        }

        private void EnsureWaveFileGenerated(WaveFormat targetFormat)
        {
            if (!this.IsWaveFileGenerated)
            {
                if (CreateWav(this.FilePath, targetFormat, out var wavPath))
                {
                    this.WaveFilePath = wavPath;
                }
            }
        }

        private bool CreateWav(string mp3Path, WaveFormat targetFormat,  out string outputWavPath)
        {
            var outputFileName = Guid.NewGuid().ToString("N");
            var cachePath = Path.Combine(Env.ProjectPath, Env.CacheFolderName);
            
            outputWavPath = Path.Combine(cachePath, $"{outputFileName}.wav");

            using var source = new AudioFileReader(mp3Path);                // float32
            
            using var resampled = new MediaFoundationResampler(source, targetFormat)
            { ResamplerQuality = 60 };

            WaveFileWriter.CreateWaveFile(outputWavPath, resampled);              // writes WAV in target format
            return true;
        }

        private void EnsureDataReady(WaveFormat targetFormat)
        {
            EnsureWaveFileGenerated(targetFormat);

            if (!IsDataReady)
            {
                if (LoadAsFloats(this.WaveFilePath, out var data, out var loadedSampleRate, out var loadedChannels, out var loadedBitsPerSample))
                {
                    if (targetFormat.SampleRate != loadedSampleRate)
                    {
                        throw new RunException($"Sample rate must be {targetFormat.SampleRate}");
                    }

                    if (targetFormat.Channels != loadedChannels)
                    {
                        throw new RunException($"Only {targetFormat.Channels} channel samples currently supported");
                    }

                    if (targetFormat.BitsPerSample != loadedBitsPerSample)
                    {
                        throw new RunException($"Only {targetFormat.BitsPerSample} bits per sample currently supported");
                    }

                    this.Data = data;
                }
            }
        }

        private bool LoadAsFloats(string path, out float[] data, out int sampleRate, out int channels, out int bitsPerSample)
        {
            using var r = new AudioFileReader(path); // 32-bit float, interleaved
            sampleRate = r.WaveFormat.SampleRate;
            channels = r.WaveFormat.Channels;
            bitsPerSample = r.WaveFormat.BitsPerSample;

            var list = new List<float>(capacity: (int)(r.Length / 4));
            var buf = ArrayPool<float>.Shared.Rent(sampleRate * channels); // ~1 sec buffer
            try
            {
                int read;
                while ((read = r.Read(buf, 0, buf.Length)) > 0)
                {
                    list.AddRange(buf.AsSpan(0, read).ToArray());
                }
            }
            finally 
            { 
                ArrayPool<float>.Shared.Return(buf); 
            }

            data = list.ToArray(); // interleaved L,R,L,R,...

            return true;
        }

        public override string ToString() => this.FileName;
    }
}
