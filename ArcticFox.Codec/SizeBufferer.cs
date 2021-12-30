using System;

namespace ArcticFox.Codec
{
    public class SizeBufferer<T>
    {
        private T[]? m_buffer;
        private int m_bufferOffset;
        private int? m_currentSize;

        protected bool m_resetSizeAfterUse = true;

        public void SetSize(int size)
        {
            m_currentSize = size;
            m_bufferOffset = 0;
        }
        
        public void ResetOffset()
        {
            m_bufferOffset = 0;
            if (m_resetSizeAfterUse)
            {
                m_currentSize = null;
            }
        }

        public bool SizeUnset()
        {
            return m_currentSize == null;
        }

        private T[] EnsureBuffer()
        {
            if (m_currentSize == null) throw new Exception("Recreating buffer while size is not known");
            var size = m_currentSize.Value;
            if (m_buffer != null && m_buffer.Length >= size) return m_buffer;
            if (m_bufferOffset != 0) throw new Exception("Recreating buffer while writing");
            m_buffer = new T[size];
            return m_buffer;
        }

        public bool ConsumeAndGet(ref ReadOnlySpan<T> data, out ReadOnlySpan<T> output)
        {
            output = default;

            if (m_currentSize == 0) return true; // done lol
            if (data.Length == 0) return false; // nothing we can do
            if (m_currentSize == null) return false; 
            var currentSize = m_currentSize.Value;
            var currentOffset = m_bufferOffset;

            var remainingSize = currentSize - currentOffset;
            if (remainingSize == 0) return false;
            
            var amountToRead = Math.Min(remainingSize, data.Length);
            var readSlice = data.Slice(0, amountToRead);
            data = data.Slice(amountToRead);

            if (currentOffset == 0 && amountToRead == currentSize)
            {
                // nothing in the buffer already, and we can read everything now. don't copy
                output = readSlice;
                m_bufferOffset += amountToRead;
                return true;
            }
            var buffer = EnsureBuffer();
            var writeSlice = new Span<T>(buffer, currentOffset, currentSize - currentOffset);
            readSlice.CopyTo(writeSlice);
            m_bufferOffset += amountToRead;

            if (m_bufferOffset == currentSize)
            {
                output = new ReadOnlySpan<T>(m_buffer, 0, currentSize);
                return true;
            }
            return false;
        }

        public bool Consume(ref ReadOnlySpan<T> data)
        {
            return ConsumeAndGet(ref data, out _);
        }
    }
}