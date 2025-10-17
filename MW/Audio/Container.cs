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
        public string F32FilePath { get; protected set; } = string.Empty;
        public bool IsF32Generated => !string.IsNullOrEmpty(this.F32FilePath);
        public bool IsDataReady => this.Data != null;
        public float[]? Data { get; protected set; } = null;

        private string? hashValue;
        public override string HashValue
        {
            get
            {
                if (hashValue is null)
                {
                    var dependencies = this.Instances.SelectMany<Instance, object>(i => [i.Offset, i.AudioSource.HashValue]);

                    hashValue = HashingTool.GenerateHash(dependencies);
                }
                return hashValue;
            }
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

        public void Add(AudioSource audioSource, double offset = 0D)
        {
            if (audioSource == this)
            {
                throw new RunException($"Cannot add a container to itself");
            }
            this.Instances.Add(Instance.CreateFrom(audioSource, offset));
        }

        public override WaveStream GetWaveStream(WaveFormat waveFormat)
        {
            EnsureF32Generated(waveFormat);

            return new F32WaveStream(this.F32FilePath, waveFormat);
        }

        public override string ToString() => $"{nameof(Container)} with {this.Instances.Count} element(s)";

        private void EnsureF32Generated(WaveFormat targetFormat)
        {
            if (!this.IsF32Generated)
            {
                if (CreateF32IfNeeded(targetFormat, out var f32Path))
                {
                    this.F32FilePath = f32Path;
                }
            }
        }

        private bool CreateF32IfNeeded(WaveFormat targetFormat, out string outputF32Path)
        {
            var outputFileName = this.HashValue;
            var cachePath = Path.Combine(Env.ProjectPath, Env.CacheFolderName);
            outputF32Path = Path.Combine(cachePath, $"{outputFileName}.f32");

            // Mix instances
            if (!File.Exists(outputF32Path))
            {
                FloatBuffer? buffer = null;

                foreach (var instance in this.Instances)
                {
                    var data = instance.AudioSource.GetData(targetFormat);
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

                if (buffer == null)
                {
                    return false;
                }

                F32Utilities.SaveF32(outputF32Path, buffer, targetFormat);
            }

            return true;
        }
    }
}
