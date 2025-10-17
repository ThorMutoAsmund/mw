using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using NAudio.Wave;

namespace MW.Helpers
{

    public sealed class F32WaveStream : WaveStream
    {
        private readonly FileStream fs;
        private readonly WaveFormat format;
        private readonly long dataStart;
        private readonly long dataLength; // in bytes

        public F32WaveStream(string path, WaveFormat waveFormat)
        {
            this.fs = File.OpenRead(path);
            this.format = waveFormat;
            this.dataStart = 0; // if you put a custom header, set it here

            this.dataLength = fs.Length;

            //byte[] buf = new byte[4];
            //int read = _fs.Read(buf, 0, 4);
            //if (read != 4) throw new EndOfStreamException();
            //_dataLength = BitConverter.ToInt32(buf, 0) * sizeof(float);
            //if (_fs.Length - _dataStart < _dataLength)
            //    _dataLength = _fs.Length - _dataStart; // trust file size if unknown
            this.Position = 0;
        }

        public override WaveFormat WaveFormat => format;
        public override long Length => dataLength;
        public override long Position
        {
            get => fs.Position - dataStart;
            set => fs.Position = dataStart + Math.Clamp(value - (value % format.BlockAlign), 0, dataLength);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            // keep to whole frames
            count -= count % format.BlockAlign;
            if (count <= 0) return 0;
            return fs.Read(buffer, offset, (int)Math.Min(count, dataLength - Position));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) fs.Dispose();
            base.Dispose(disposing);
        }
    }
}
