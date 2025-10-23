using System;
using System.Buffers;

namespace MW.Helpers
{
    using System;
    using System.Buffers;

    public sealed class FloatBuffer : IDisposable
    {
        private float[] _buffer;
        private int _count;
        private bool _pooled; // true if _buffer came from ArrayPool

        public FloatBuffer(int initialCapacity = 4096)
        {
            _buffer = ArrayPool<float>.Shared.Rent(initialCapacity);
            _count = 0;
            _pooled = true;
        }

        /// <summary>
        /// Wrap an existing array without copying. No ownership is taken (not returned to pool).
        /// If you later append past its capacity, the buffer will migrate to a pooled array.
        /// </summary>
        public static FloatBuffer Wrap(float[] array, int count = -1)
        {
            if (array is null) throw new ArgumentNullException(nameof(array));
            if (count < 0) count = array.Length;
            if ((uint)count > (uint)array.Length) throw new ArgumentOutOfRangeException(nameof(count));
            return new FloatBuffer(array, count);
        }

        private FloatBuffer(float[] external, int count)
        {
            _buffer = external;
            _count = count;
            _pooled = false; // don't return to pool on Dispose
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
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (count == 0) return;

            Ensure(count);
            new Span<float>(_buffer, _count, count).Clear();
            _count += count;
        }

        /// <summary>Mix src into the buffer at offset: dst[i+offset] += gain * src[i]. Extends Count if needed.</summary>
        public void Add(int offset, ReadOnlySpan<float> src, float gain = 1f)
        {
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
            int end = offset + src.Length;
            EnsureCapacity(end);
            if (end > _count) _count = end;

            var dst = _buffer.AsSpan(offset, src.Length);
            if (gain == 1f)
            {
                for (int i = 0; i < src.Length; i++) dst[i] += src[i];
            }
            else
            {
                for (int i = 0; i < src.Length; i++) dst[i] += src[i] * gain;
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
            if (needed <= _buffer.Length) return;
            GrowTo(needed);
        }

        private void Ensure(int additional)
        {
            int needed = _count + additional;
            if (needed <= _buffer.Length) return;
            GrowTo(needed);
        }

        private void GrowTo(int needed)
        {
            int newCap = Math.Max(needed, _buffer.Length * 2);
            var newBuf = ArrayPool<float>.Shared.Rent(newCap);
            Array.Copy(_buffer, 0, newBuf, 0, _count);

            // Return old buffer only if it was from the pool
            if (_pooled) ArrayPool<float>.Shared.Return(_buffer, clearArray: false);

            _buffer = newBuf;
            _pooled = true; // from now on, owned by pool
        }

        public void Dispose()
        {
            if (_pooled && _buffer.Length != 0)
                ArrayPool<float>.Shared.Return(_buffer, clearArray: false);

            _buffer = Array.Empty<float>();
            _count = 0;
            _pooled = false;
        }
    }

}
