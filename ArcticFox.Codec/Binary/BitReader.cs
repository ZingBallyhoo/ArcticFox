using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;

namespace ArcticFox.Codec.Binary
{
    public ref partial struct BitReader
    {        
        private readonly ReadOnlySpan<byte> m_data;
        public int m_dataOffset { get; private set; }
        public int m_dataLength => m_data.Length;

        private byte m_bitValue;
        public byte m_bitPositionInByte { get; private set; } = 8;

        public bool m_notReadingBits => m_bitPositionInByte == 8;

        public byte m_bitsRemainingInCurrentByte => m_bitPositionInByte == 8 ? (byte)8 : (byte)(8 - m_bitPositionInByte);
        public uint m_fullBitOffset => (uint)m_dataOffset * 8u - (m_notReadingBits ? 0u : m_bitsRemainingInCurrentByte);

        public BitReader(ReadOnlySpan<byte> data)
        {
            m_data = data;
            m_dataOffset = 0;
            m_bitValue = 0;
        }

        private void ClearBit()
        {
            m_bitPositionInByte = 8;
        }

        public byte ReadByte()
        {
            ClearBit();
            return m_data[m_dataOffset++];
        }

        public sbyte ReadSByte() => (sbyte) ReadByte();
        
        public void ReadBytesTo(Span<byte> output, int count)
        {
            ReadBytesTo(output.Slice(0, count));
        }

        public void ReadBytesTo(Span<byte> output)
        {
            var readSpan = ReadBytes(output.Length);
            readSpan.CopyTo(output);
        }
        
        public ReadOnlySpan<byte> ReadBytes(int count)
        {
            ClearBit();
            var span = m_data.Slice(m_dataOffset, count);
            m_dataOffset += count;
            return span;
        }

        private void FetchNextBits()
        {
            m_bitValue = ReadByte();
            m_bitPositionInByte = 0;
        }

        public bool ReadBit()
        {
            if (m_notReadingBits) FetchNextBits();

            var value = (m_bitValue & (1 << m_bitPositionInByte)) != 0;
            
            m_bitPositionInByte++;
            return value;
        }

        internal byte ReadBitsFromCurrent(byte count)
        {
            if (m_notReadingBits) FetchNextBits();

            var remainingBits = 8 - m_bitPositionInByte;
            Debug.Assert(remainingBits >= count);

            var val = ExtractBitRange(m_bitValue, m_bitPositionInByte, count);
            m_bitPositionInByte += count;
            
            Debug.Assert(m_bitPositionInByte <= 8);

            return val;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte ExtractBitRange(byte value, byte start, byte length)
        {
            // todo: is this faster in all cases?
            if (Bmi1.IsSupported)
            {
                return (byte)Bmi1.BitFieldExtract(value, start, length);
            }

            // few more instructions... idk
            // var val = (byte)(value >> start);
            // val &= (byte)((byte)(1 << length) - 1);
            // return val;
            
            return (byte)((value >> start) & (byte)((1u << length) - 1u));
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
            var writer = new BitWriter(span);
            writer.WriteBits(ref this, bitCount);
            writer.FlushBit();
        }

        public void SkipBytes(int count)
        {
            ClearBit();
            m_dataOffset += count;
        }

        public void SeekByte(uint position) => SeekBit(position * 8);

        public void SeekBit(uint position)
        {
            if (position == m_fullBitOffset)
            {
                // already at right pos
                return;
            }
            
            var bytePos = position >> 3;
            var bitPos = position - (bytePos << 3);
            
            if (bytePos > m_dataLength)
            {
                throw new IndexOutOfRangeException();
            }
            
            if (bytePos == m_dataOffset-1 && !m_notReadingBits && bitPos != 0)
            {
                m_bitPositionInByte = (byte)bitPos;
                return;
            }
            
            m_dataOffset = (int)bytePos;
            
            if (bitPos != 0)
            {
                FetchNextBits();
                m_bitPositionInByte = (byte)bitPos;
            } else
            {
                ClearBit();
            }
        }
    }
}