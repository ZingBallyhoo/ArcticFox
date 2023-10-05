using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace ArcticFox.Codec.Binary
{
    public ref partial struct BitWriter
    {
        public readonly Span<byte> m_output;
        public int m_dataOffset { get; private set; }
        public int m_dataLength => m_output.Length;

        private ref byte m_bitValue => ref m_output[m_dataOffset];
        public byte m_bitPositionInByte { get; private set; }

        public int m_effectiveByteOffset => m_dataOffset + (m_bitPositionInByte != 0 ? 1 : 0); // consider pending bits
        public byte m_bitsRemainingInCurrentByte => m_bitPositionInByte == 8 ? (byte)8 : (byte)(8 - m_bitPositionInByte);
        public uint m_fullBitOffset => (uint)m_dataOffset * 8u + m_bitPositionInByte;

        public BitWriter(Span<byte> output)
        {
            m_output = output;
            m_dataOffset = 0;
            //m_bitValue = 0;
            m_bitPositionInByte = 0;
        }

        public void WriteBit(bool bit)
        {
            if (bit)
            {
                m_bitValue |= (byte) (1 << m_bitPositionInByte);
            }
            else
            {
                m_bitValue &= (byte)~(byte) (1 << m_bitPositionInByte);
            }
            m_bitPositionInByte++;
            
            if (m_bitPositionInByte == 8) FlushBit();
        }

        public void WriteByte(byte value)
        {
            FlushBit();
            m_output[m_dataOffset++] = value;
        }

        public void WriteSByte(sbyte value) => WriteByte((byte)value); 

        private void WriteBitsToCurrent(byte bits, byte bitCount)
        {
            var remainingBitsThisByte = (byte)(8 - m_bitPositionInByte);
            Debug.Assert(remainingBitsThisByte >= bitCount);
            
            // todo: assumes caller is sane and doesn't pass garbage bits. could mask using bitcount

            var maskToRemove = (byte)((1 << bitCount) - 1) << m_bitPositionInByte;

            m_bitValue &= (byte)~maskToRemove;
            m_bitValue |= (byte)(bits << m_bitPositionInByte);
            m_bitPositionInByte += bitCount;
            
            Debug.Assert(m_bitPositionInByte <= 8);
            if (m_bitPositionInByte == 8) FlushBit();
        }

        public unsafe void WriteBits<T>(T obj, uint bitCount) where T : unmanaged
        {
            Debug.Assert(sizeof(T) * 8 >= bitCount);

            var objSpan = new Span<byte>((byte*)&obj, sizeof(T));
            var reader = new BitReader(objSpan);
            WriteBits(ref reader, bitCount);
        }
        
        public void WriteBits(ref BitReader buffer, uint bitCount)
        {
            while (bitCount > 0)
            {
                var bitsCanRead = buffer.m_bitsRemainingInCurrentByte;
                var bitsCanWrite = m_bitsRemainingInCurrentByte;

                var maxBitsToReadGivenAlignment = (byte)Math.Min(bitCount, Math.Min(bitsCanRead, bitsCanWrite));
                Debug.Assert(maxBitsToReadGivenAlignment > 0);

                if (maxBitsToReadGivenAlignment == 8)
                {
                    // we can blit the data directly into the output
                    
                    var bytesThisOperation = bitCount >> 3;
                    buffer.ReadBytesTo(m_output.Slice(m_dataOffset), (int)bytesThisOperation);
                    m_dataOffset += (int)bytesThisOperation;
                    bitCount -= bytesThisOperation * 8;
                    continue;
                }

                if (bitsCanRead == 8 && bitCount >= 16)
                {
                    // the reader is currently aligned to a byte boundary
                    // 1. load as much data as we can shift in one operation
                    // 2. align the writer using the start bits of what we read
                    // 3. shift away the start bits
                    // 4. blit as much as we can between the 2 aligned streams (recurse into this func)
                    // 5. Read/WriteBitsFromCurrent the remaining bits (recurse into this func)
                    
                    Debug.Assert(buffer.m_notReadingBits); // reader should be aligned
                    Debug.Assert(m_bitPositionInByte != 0); // writer should not be aligned (or above path would be used)

                    byte maxByteCount;
                    if (Avx2.IsSupported)
                    {
                        maxByteCount = (byte)Vector256<byte>.Count;
                    } else if (Sse2.IsSupported)
                    {
                        maxByteCount = (byte)Vector128<byte>.Count;
                    } else
                    {
                        maxByteCount = sizeof(ulong);
                    }
                    var vectorBytesToRead = Math.Min(bitCount >> 3, maxByteCount);
                    var bitsToReadAsVector = vectorBytesToRead * 8;
                    
                    unsafe
                    {
                        var vector = Vector256<ulong>.Zero;
                        var vectorSpan = new Span<byte>(&vector, maxByteCount);
                        buffer.ReadBytesTo(vectorSpan, (int)vectorBytesToRead);
                        
                        var startBitsReader = new BitReader(vectorSpan);
                        var startBits = startBitsReader.ReadBitsFromCurrent(maxBitsToReadGivenAlignment);
                        WriteBitsToCurrent(startBits, maxBitsToReadGivenAlignment);
                        
                        // we have now aligned the writer to a byte boundary
                        Debug.Assert(m_bitPositionInByte == 0);
                        
                        var remainingBits = bitsToReadAsVector - maxBitsToReadGivenAlignment;

                        // remove the bits we've already written
                        if (Vector256<float>.IsSupported && Avx2.IsSupported && remainingBits > 128)
                        {
                            // https://stackoverflow.com/questions/25248766/emulating-shifts-on-32-bytes-with-avx
                            // https://codereview.stackexchange.com/questions/253761/optimizing-bit-matching-performance-using-avx-compiler-intrinsic
                            // https://stackoverflow.com/questions/35001722/global-bitwise-shift-of-128-256-512-bit-registry-using-intrinsics/35002285
                            
                            var alreadyReadBits = startBitsReader.m_bitPositionInByte;
                            var otherShift = (byte)(64 - alreadyReadBits);
                            
                            var innerCarry = Avx2.ShiftLeftLogical(vector, otherShift);
                            var rotate = Avx2.Permute4x64(innerCarry, 0x39);
                            var innerCarry2 = Avx2.Blend(Vector256<uint>.Zero, Unsafe.As<Vector256<ulong>, Vector256<uint>>(ref rotate), 0x3F);
                            innerCarry = innerCarry2.AsUInt64();
                            
                            vector = Vector256.ShiftRightLogical(vector, alreadyReadBits);
                            vector = Vector256.BitwiseOr(vector, innerCarry);
                        } else if (Vector128<float>.IsSupported && Sse2.IsSupported && remainingBits > 64)
                        {
                            // https://mischasan.wordpress.com/2012/12/26/sse2-bit-shift/
                            // https://mischasan.wordpress.com/2013/04/07/the-c-preprocessor-not-as-cryptic-as-youd-think/
                            
                            ref var sseVec = ref Unsafe.As<Vector256<ulong>, Vector128<ulong>>(ref vector);

                            var alreadyReadBits = startBitsReader.m_bitPositionInByte;
                            var otherShift = (byte)(64 - alreadyReadBits);
                            
                            var innerCarry = Sse2.ShiftLeftLogical(
                                Sse2.ShiftRightLogical128BitLane(sseVec, 8), otherShift);
                            sseVec = Vector128.ShiftRightLogical(sseVec, alreadyReadBits);
                            sseVec = Vector128.BitwiseOr(sseVec, innerCarry);
                        } else
                        {
                            Debug.Assert(remainingBits <= 64);
                            
                            ref var ul = ref Unsafe.As<Vector256<ulong>, ulong>(ref vector);
                            ul >>= startBitsReader.m_bitPositionInByte;
                        }
                        
                        // blit n bytes, then Read/WriteBitsFromCurrent remainder
                        var blitReader = new BitReader(vectorSpan);
                        WriteBits(ref blitReader, remainingBits);
                    }
                    
                    bitCount -= bitsToReadAsVector;
                    continue;
                }

                // read what as much as we can in one operation. 2 operations = 8 bits
                // each operation alternates size
                // e.g 1, 7, 1, 7 or 5, 3, 5, 3
                var readBits = buffer.ReadBitsFromCurrent(maxBitsToReadGivenAlignment);
                WriteBitsToCurrent(readBits, maxBitsToReadGivenAlignment);

                bitCount -= maxBitsToReadGivenAlignment;
            }
        }

        public void FlushBit()
        {
            if (m_bitPositionInByte == 0) return;
            m_bitPositionInByte = 0;
            m_dataOffset++;
        }

        public Span<byte> GetSpanOfNextBytes(int size)
        {
            FlushBit();
            var span = m_output.Slice(m_dataOffset, size);
            m_dataOffset += size;
            return span;
        }
        
        public void WriteBytes(ReadOnlySpan<byte> data)
        {
            var outputSpan = GetSpanOfNextBytes(data.Length);
            data.CopyTo(outputSpan);
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
            
            m_dataOffset = (int)bytePos;
            m_bitPositionInByte = (byte)bitPos;
        }
    }
}