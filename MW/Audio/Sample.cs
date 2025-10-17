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
        public string FilePath { get; protected set; } = string.Empty;
        public string FileName { get; protected set; } = string.Empty;
        public string F32FilePath { get; protected set; } = string.Empty;
        public bool IsWaveFileGenerated => !string.IsNullOrEmpty(this.F32FilePath);
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
            this.FileName = Path.GetFileName(existingFilePath);

            //if (Project.IsWavFile(this.FilePath))
            //{
            //    this.WaveFilePath = this.FilePath;
            //}
        }

        public override WaveStream GetWaveStream(WaveFormat waveFormat)
        {
            EnsureF32FileGenerated(waveFormat);

            return new RawFloatFileWaveStream(this.F32FilePath, waveFormat);
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

        private void EnsureF32FileGenerated(WaveFormat targetFormat)
        {
            if (!this.IsWaveFileGenerated)
            {
                if (CreateF32IfNeeded(this.FilePath, targetFormat, out var f32Path))
                {
                    this.F32FilePath = f32Path;
                }
            }
        }

        private bool CreateF32IfNeeded(string inputPath, WaveFormat targetFormat, out string outputWavPath)
        {
            var outputFileName = this.HashValue;
            var cachePath = Path.Combine(Env.ProjectPath, Env.CacheFolderName);            
            outputWavPath = Path.Combine(cachePath, $"{outputFileName}.f32");

            if (!File.Exists(outputWavPath))
            {
                using var source = new AudioFileReader(inputPath);          // decodes to 32-bit float bytes
                using var resampler = new MediaFoundationResampler(source, targetFormat)
                { ResamplerQuality = resamplerQuality };

                RawFloatFileWaveStream.SaveF32(outputWavPath, resampler.ToSampleProvider(), targetFormat);
            }

            return true;
        }

        private void EnsureDataReady(WaveFormat targetFormat)
        {
            EnsureF32FileGenerated(targetFormat);

            if (!IsDataReady)
            {
                this.Data = RawFloatFileWaveStream.ReadF32(this.F32FilePath, targetFormat);
            }
        }

        //private bool LoadAsFloats(string path, out float[] data, WaveFormat targetFormat)
        //{
        //    using var r = new RawFloatFileWaveStream(path, targetFormat); // 32-bit float, interleaved

        //    var list = new List<float>(capacity: (int)(r.Length / 4));
        //    var buf = ArrayPool<float>.Shared.Rent(targetFormat.SampleRate * targetFormat.Channels); // ~1 sec buffer
        //    try
        //    {
        //        int read;
        //        while ((read = r.Read(buf, 0, buf.Length)) > 0)
        //        {
        //            list.AddRange(buf.AsSpan(0, read).ToArray());
        //        }
        //    }
        //    finally 
        //    { 
        //        ArrayPool<float>.Shared.Return(buf); 
        //    }

        //    data = list.ToArray(); // interleaved L,R,L,R,...

        //    return true;
        //}

        public override string ToString() => this.FileName;
    }
}
