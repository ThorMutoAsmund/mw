using MW.Helpers;
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
        public string FilePath { get; init; } = string.Empty;
        public string FileName => Path.GetFileName(this.FilePath);
        public string F32FilePath { get; protected set; } = string.Empty;
        public bool IsF32Generated => !string.IsNullOrEmpty(this.F32FilePath);
        public bool IsDataReady => this.Data != null;
        public float[]? Data { get; protected set; } = null;

        private const int resamplerQuality = 60;
        public override string HashValue { get; }
        private string toStringValue;
        private bool isGeneratedSample;
        public Sample(string existingFilePath)
        {
            this.FilePath = existingFilePath;
            this.HashValue = HashingTool.GenerateHash([this.FileName, resamplerQuality, new FileInfo(this.FilePath).Length]);
            this.toStringValue = this.FileName;
        }

        public Sample(float[] data, string hashValue)
        {
            this.isGeneratedSample = true;
            this.Data = data;
            this.HashValue = hashValue;
            this.toStringValue = hashValue;
        }

        public override float[] GetData()
        {
            EnsureF32Generated();

            if (!IsDataReady)
            {
                this.Data = F32Utilities.ReadF32(this.F32FilePath, Env.Song.Format);
            }

            return this.Data!;
        }

        public override WaveStream GetWaveStream()
        {
            EnsureF32Generated();

            return new F32WaveStream(this.F32FilePath, Env.Song.Format);
        }

        private void EnsureF32Generated()
        {
            if (!this.IsF32Generated)
            {
                if (CreateF32IfNeeded(this.FilePath, out var f32Path))
                {
                    this.F32FilePath = f32Path;
                }
            }
        }

        private bool CreateF32IfNeeded(string inputPath, out string outputF32Path)
        {
            var cachePath = Path.Combine(Env.ProjectPath, Env.CacheFolderName);            
            outputF32Path = Path.Combine(cachePath, $"{this.HashValue}.f32");

            if (!File.Exists(outputF32Path))
            {
                if (this.isGeneratedSample)
                {
                    if (!this.IsDataReady)
                    {
                        throw new RunException("Cannot save generated sample if no data");
                    }

                    F32Utilities.SaveF32(outputF32Path, FloatBuffer.Wrap(this.Data!), Env.Song.Format);
                }
                else
                {
                    using var source = new AudioFileReader(inputPath);          // decodes to 32-bit float bytes
                    using var resampler = new MediaFoundationResampler(source, Env.Song.Format)
                    { ResamplerQuality = resamplerQuality };

                    F32Utilities.SaveF32(outputF32Path, resampler.ToSampleProvider(), Env.Song.Format);
                }
            }

            return true;
        }


        public override string ToString() => this.toStringValue;
    }
}
