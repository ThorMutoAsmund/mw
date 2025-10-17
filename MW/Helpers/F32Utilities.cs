using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using NAudio.Wave;

namespace MW.Helpers
{

    public static class F32Utilities
    {
        public static float[] ReadF32(string path, WaveFormat waveFormat)
        {
            if (path is null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            long byteLen = new FileInfo(path).Length;
            if (byteLen % sizeof(float) != 0)
            {
                throw new InvalidDataException("File length is not a multiple of 4 bytes (float32).");
            }

            // NOTE: this allocates a single float[] and fills it directly
            int sampleCount = checked((int)(byteLen / sizeof(float)));
            var samples = new float[sampleCount];

            using var fs = File.OpenRead(path);
            var dest = MemoryMarshal.AsBytes(samples.AsSpan()); // Span<byte> view over the float[]
            int totalRead = 0;
            while (totalRead < dest.Length)
            {
                int n = fs.Read(dest.Slice(totalRead));
                if (n == 0) throw new EndOfStreamException("Unexpected EOF while reading .f32 data.");
                totalRead += n;
            }

            return samples; // interleaved L,R,L,R,...
        }

        public static void SaveF32(string outputPath, FloatBuffer buffer, WaveFormat waveFormat, int offset = 0, int? count = null)
        {
            if (buffer is null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            var span = buffer.Span;                         // ReadOnlySpan<float> over [0..Count)
            if (offset < 0 || offset > span.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            int len = count ?? (span.Length - offset);
            if (len < 0 || offset + len > span.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            using var fs = File.Create(outputPath);
            ReadOnlySpan<byte> bytes = MemoryMarshal.AsBytes(span.Slice(offset, len));
            fs.Write(bytes);
        }

        public static void SaveF32(string outputPath, ISampleProvider sampleProvider, WaveFormat waveFormat, int bufferMillis = 1000)
        {
            if (sampleProvider is null)
            {
                throw new ArgumentNullException(nameof(sampleProvider));
            }

            var wf = sampleProvider.WaveFormat;
            if (wf.BitsPerSample != waveFormat.BitsPerSample)
            {
                throw new ArgumentException($"ISampleProvider must be {waveFormat.BitsPerSample}-bit float.", nameof(sampleProvider));
            }

            // ~bufferMillis of audio (samples, not frames). Minimum keeps small reads efficient.
            int samplesPerBuffer = Math.Max(4096, wf.SampleRate * wf.Channels * bufferMillis / 1000);
            float[] fbuf = ArrayPool<float>.Shared.Rent(samplesPerBuffer);

            try
            {
                using var fs = File.Create(outputPath);

                int read;
                while ((read = sampleProvider.Read(fbuf, 0, fbuf.Length)) > 0)
                {
                    ReadOnlySpan<byte> bytes = MemoryMarshal.AsBytes(fbuf.AsSpan(0, read));
                    fs.Write(bytes); // zero-copy write
                }
            }
            finally
            {
                ArrayPool<float>.Shared.Return(fbuf);
            }
        }
    }
}
