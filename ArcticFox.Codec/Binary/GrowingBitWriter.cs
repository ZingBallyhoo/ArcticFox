using System;
using System.Buffers;
using CommunityToolkit.HighPerformance.Buffers;

namespace ArcticFox.Codec.Binary
{
    public ref partial struct GrowingBitWriter
    {
        private IMemoryOwner<byte> m_memory;
        private BitWriter m_writer;
        public int m_memorySize { get; private set; }
        
        private int m_memorySizeInBits => m_memorySize * 8;

        public int m_effectiveByteOffset => m_writer.m_effectiveByteOffset;
        public int m_dataOffset => m_writer.m_dataOffset;
        public uint m_fullBitOffset => m_writer.m_fullBitOffset;

        public GrowingBitWriter()
        {
            m_memorySize = 1024;
            m_memory = MemoryOwner<byte>.Allocate(m_memorySize);
            m_writer = new BitWriter(m_memory.Memory.Span);
        }
        
        public ReadOnlyMemory<byte> GetData()
        {
            m_writer.FlushBit();
            return m_memory.Memory.Slice(0, m_writer.m_dataOffset);
        }
        
        public void WriteBit(bool bit)
        {
            if (m_writer.m_fullBitOffset + 1 > m_memorySizeInBits)
            {
                Grow();
            }
            m_writer.WriteBit(bit);
        }
        
        public void WriteByte(byte value)
        {
            if (m_effectiveByteOffset + 1 > m_memorySize)
            {
                Grow();
            }
            m_writer.WriteByte(value);
        }
        
        public void WriteSByte(sbyte value) => WriteByte((byte)value);
        
        public void WriteBits(ref BitReader buffer, uint bitCount)
        {
            var lengthInBitsAfterWrite = m_writer.m_fullBitOffset + bitCount;
            while (lengthInBitsAfterWrite > m_memorySizeInBits)
            {
                Grow();
            }
            m_writer.WriteBits(ref buffer, bitCount);
        }
        
        public void WriteBits<T>(T obj, uint bitCount) where T : unmanaged
        {
            var lengthInBitsAfterWrite = m_writer.m_fullBitOffset + bitCount;
            while (lengthInBitsAfterWrite > m_memorySizeInBits)
            {
                Grow();
            }
            m_writer.WriteBits(obj, bitCount);
        }

        public Span<byte> GetSpanOfNextBytes(int size)
        {
            var lengthAfterWrite = m_effectiveByteOffset + size;
            while (lengthAfterWrite > m_memorySize)
            {
                Grow();
            }
            return m_writer.GetSpanOfNextBytes(size);
        }
        
        public void WriteBytes(ReadOnlySpan<byte> data)
        {
            var outputSpan = GetSpanOfNextBytes(data.Length);
            data.CopyTo(outputSpan);
        }

        private void Grow()
        {
            m_memorySize *= 2;

            var currentMemory = m_memory;
            var currentMemoryReader = new BitReader(currentMemory.Memory.Span);
            
            var newMemory = MemoryOwner<byte>.Allocate(m_memorySize);
            var newWriter = new BitWriter(newMemory.Memory.Span);
            
            newWriter.WriteBits(ref currentMemoryReader, m_writer.m_fullBitOffset);
            currentMemory.Dispose();
            
            m_memory = newMemory;
            m_writer = newWriter; // preserve bit pos etc
        }
        
        public void SeekByte(uint position) => m_writer.SeekByte(position);
        public void SeekBit(uint position) => m_writer.SeekBit(position);

        public void Dispose()
        {
            m_memory.Dispose();
        }
    }
}