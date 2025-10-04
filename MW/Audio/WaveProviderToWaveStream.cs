using NAudio.Wave;

namespace MW.Audio
{
    public sealed class WaveProviderToWaveStream : WaveStream
    {
        private readonly IWaveProvider source;
        private long position; // bytes read so far

        public WaveProviderToWaveStream(IWaveProvider source) => this.source = source;

        public override WaveFormat WaveFormat => source.WaveFormat;

        // Unknown length – return a big number (or Int64.MaxValue if you prefer)
        public override long Length => int.MaxValue;

        public override long Position
        {
            get => position;
            set => throw new NotSupportedException("Cannot set position on a non-seekable IWaveProvider");
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int read = source.Read(buffer, offset, count);
            position += read;
            return read;
        }
    }
}