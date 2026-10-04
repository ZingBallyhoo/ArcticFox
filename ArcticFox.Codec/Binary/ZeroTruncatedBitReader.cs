using System;
using System.IO;

namespace ArcticFox.Codec.Binary
{
    public ref partial struct ZeroTruncatedBitReader : IBitReader
    {
        private BitReader m_reader;

        private int m_remainingBytes => m_reader.m_dataLength - m_reader.m_dataOffset;
        private uint m_bitLength => (uint)m_reader.m_dataLength * 8u;
        private uint m_remainingBits => m_bitLength - m_reader.m_fullBitOffset;

        public ZeroTruncatedBitReader(ReadOnlySpan<byte> data)
        {
            m_reader = new BitReader(data);
        }
        
        public byte ReadByte()
        {
            if (m_remainingBytes <= 0) return 0;
            return m_reader.ReadByte();
        }

        public sbyte ReadSByte() => (sbyte) ReadByte();
        
        public void ReadBytesTo(Span<byte> output, int count)
        {
            ReadBytesTo(output.Slice(0, count));
        }
        
        public void ReadBytesTo(Span<byte> output)
        {
            var readableSize = Math.Min(output.Length, m_remainingBytes);
            
            m_reader.ReadBytesTo(output, readableSize);
            output.Slice(readableSize).Clear();
        }
        
        public bool ReadBit()
        {
            if (m_remainingBits <= 0) return false;
            return m_reader.ReadBit();
        }

        public unsafe T ReadBits<T>(uint bitCount) where T : unmanaged
        {
            if (sizeof(T) * 8 < bitCount) throw new InvalidDataException();

            var obj = new T();
            var objSpan = new Span<byte>((byte*)&obj, sizeof(T));
            ReadBits(objSpan, bitCount);

            return obj;
        }
        
        public void ReadBits(Span<byte> span, uint bitCount)
        {
            var readableSize = Math.Min(bitCount, m_remainingBits);
            
            var writer = new BitWriter(span);
            writer.WriteBits(ref m_reader, readableSize);
            
            // pretty awkward, but i haven't found a better way of doing this.
            // bits between readableSize and bitCount need to be zeroed in the destination.
            // there could be trailing data, and that needs to be left alone.
            // todo: use a bigger value type.
            var padBits = bitCount - readableSize;
            while (padBits > 0)
            {
                var chunk = Math.Min(padBits, sizeof(ulong) * 8);
                writer.WriteBits(0ul, chunk);
                padBits -= chunk;
            }
        }
        
        public void SkipBytes(int count)
        {
            var readableSize = Math.Min(count, m_remainingBytes);
            m_reader.SkipBytes(readableSize);
        }
        
        public void SeekByte(uint position) => SeekBit(position * 8);

        public void SeekBit(uint position)
        {
            m_reader.SeekBit(Math.Min(position, m_bitLength));
        }

        /*public double ReadDoubleBigEndian()
        {
            var remainingBytes = m_remainingBytes;
            if (remainingBytes == 0) return 0;
            if (remainingBytes >= sizeof(double))
            {
                return m_reader.ReadDoubleBigEndian();
            }
            Span<byte> buffer = stackalloc byte[sizeof(double)];
            ReadBytesTo(buffer, sizeof(double));
            
            var reader = new BitReader(buffer);
            return reader.ReadDoubleBigEndian();
        }*/
    }
}