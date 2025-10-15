using System;
using System.Buffers;

namespace MW.Helpers
{
    public sealed class FloatBuffer : IDisposable
    {
        private float[] _buffer;
        private int _count;

        public FloatBuffer(int initialCapacity = 4096)
        {
            _buffer = ArrayPool<float>.Shared.Rent(initialCapacity);
            _count = 0;
        }

        public int Count => _count;
        public ReadOnlySpan<float> Span => new ReadOnlySpan<float>(_buffer, 0, _count);

        public void Append(float value)
        {
            Ensure(1);
            _buffer[_count++] = value;
        }

        public void Append(ReadOnlySpan<float> src)
        {
            Ensure(src.Length);
            src.CopyTo(new Span<float>(_buffer, _count, src.Length));
            _count += src.Length;
        }

        public void AppendZeros(int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (count == 0)
            {
                return;
            }

            Ensure(count);
            new Span<float>(_buffer, _count, count).Clear(); // fast memset(0)
            _count += count;
        }

        public void Add(int offset, ReadOnlySpan<float> src, float gain = 1f)
        {
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));

            int end = offset + src.Length;
            EnsureCapacity(end);                 // grow underlying array if needed

            // If we're writing past current logical end, extend _count
            if (end > _count) _count = end;

            var dst = _buffer.AsSpan(offset, src.Length);

            if (gain == 1f)
            {
                for (int i = 0; i < src.Length; i++)
                    dst[i] += src[i];
            }
            else
            {
                for (int i = 0; i < src.Length; i++)
                    dst[i] += src[i] * gain;
            }
        }

        public float[] ToArray()
        {
            var arr = new float[_count];
            Array.Copy(_buffer, 0, arr, 0, _count);
            return arr;
        }

        private void EnsureCapacity(int needed)
        {
            if (needed <= _buffer.Length)
            {
                return;
            }

            int newCap = Math.Max(needed, _buffer.Length * 2);
            var newBuf = ArrayPool<float>.Shared.Rent(newCap);
            Array.Copy(_buffer, 0, newBuf, 0, _count);
            ArrayPool<float>.Shared.Return(_buffer, clearArray: false);
            _buffer = newBuf;
        }

        private void Ensure(int additional)
        {
            int needed = _count + additional;
            if (needed <= _buffer.Length)
            {
                return;
            }

            int newCap = Math.Max(needed, _buffer.Length * 2);
            var newBuf = ArrayPool<float>.Shared.Rent(newCap);
            Array.Copy(_buffer, 0, newBuf, 0, _count);
            ArrayPool<float>.Shared.Return(_buffer, clearArray: false);
            _buffer = newBuf;
        }

        public void Dispose()
        {
            ArrayPool<float>.Shared.Return(_buffer, clearArray: false);
            _buffer = Array.Empty<float>();
            _count = 0;
        }
    }
}
