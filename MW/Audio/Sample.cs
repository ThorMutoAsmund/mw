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

        private string? hashValue;
        public override string HashValue
        {
            get
            {
                if (hashValue is null)
                {
                    hashValue = HashingTool.GenerateHash([this.FileName, resamplerQuality, new FileInfo(this.FilePath).Length]);
                }
                return hashValue;
            }
        }

        public Sample(string existingFilePath)
        {
            this.FilePath = existingFilePath;
        }

        public override float[] GetData(WaveFormat targetFormat)
        {
            EnsureF32Generated(targetFormat);

            if (!IsDataReady)
            {
                this.Data = F32Utilities.ReadF32(this.F32FilePath, targetFormat);
                //return Array.Empty<float>();
            }

            return this.Data!;
        }

        public override WaveStream GetWaveStream(WaveFormat waveFormat)
        {
            EnsureF32Generated(waveFormat);

            return new F32WaveStream(this.F32FilePath, waveFormat);
        }

        private void EnsureF32Generated(WaveFormat targetFormat)
        {
            if (!this.IsF32Generated)
            {
                if (CreateF32IfNeeded(this.FilePath, targetFormat, out var f32Path))
                {
                    this.F32FilePath = f32Path;
                }
            }
        }

        private bool CreateF32IfNeeded(string inputPath, WaveFormat targetFormat, out string outputF32Path)
        {
            var outputFileName = this.HashValue;
            var cachePath = Path.Combine(Env.ProjectPath, Env.CacheFolderName);            
            outputF32Path = Path.Combine(cachePath, $"{outputFileName}.f32");

            if (!File.Exists(outputF32Path))
            {
                using var source = new AudioFileReader(inputPath);          // decodes to 32-bit float bytes
                using var resampler = new MediaFoundationResampler(source, targetFormat)
                { ResamplerQuality = resamplerQuality };

                F32Utilities.SaveF32(outputF32Path, resampler.ToSampleProvider(), targetFormat);
            }

            return true;
        }


        public override string ToString() => this.FileName;
    }
}
