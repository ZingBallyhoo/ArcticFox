using System;
using ArcticFox.Codec.Binary;
using Xunit;

namespace ArcticFox.Tests
{
    public class BinaryTests
    {
        [Fact]
        public void GrowableBufferHandlesDanglingByte()
        {
            using var growableBuffer = new GrowingBitWriter();
            growableBuffer.WriteInt32LittleEndian(100);
            
            growableBuffer.WriteBit(true); // 1
            growableBuffer.WriteBit(true);
            growableBuffer.WriteBit(true);
            growableBuffer.WriteBit(true);
            growableBuffer.WriteBit(true);
            growableBuffer.WriteBit(true);
            growableBuffer.WriteBit(true);
            growableBuffer.WriteBit(true); // 8
            growableBuffer.WriteBit(false); // 9
            Assert.Equal(9u + 32u, growableBuffer.m_fullBitOffset);

            var data = growableBuffer.GetData();
            Assert.Equal(6, growableBuffer.m_dataOffset);
            Assert.Equal(6, data.Length);

            var reader = new BitReader(data.Span);
            Assert.Equal(100, reader.ReadInt32LittleEndian());
            Assert.True(reader.ReadBit()); // 1
            Assert.True(reader.ReadBit());
            Assert.True(reader.ReadBit());
            Assert.True(reader.ReadBit());
            Assert.True(reader.ReadBit());
            Assert.True(reader.ReadBit());
            Assert.True(reader.ReadBit());
            Assert.True(reader.ReadBit()); // 8
            Assert.False(reader.ReadBit()); // 9
            Assert.Equal(9u + 32u, reader.m_fullBitOffset);
        }

        [Fact]
        public void CopyUnaligned()
        {
            var reader = new BitReader(new byte[] {0xFE, 0xFF, 0xFF, 0xFF, 0x1});
            Assert.False(reader.ReadBit());
            Assert.Equal(0xFFFFFFFF, reader.ReadBits<uint>(32)); // bits 1 to 33
            Assert.Equal(0, reader.ReadBits<byte>(7));
        }
        
        [Fact]
        public void ExactValueIsPreserved()
        {
            const uint cookie = 1234567891;
            
            var writer = new BitWriter(new byte[1+sizeof(uint)+sizeof(ulong)+4*sizeof(ulong)]);
            writer.WriteBit(false);
            writer.WriteBits(cookie, 32);
            writer.WriteBits<ulong>(cookie, 64);
            writer.WriteBits<CookieStruct>(new CookieStruct
            {
                m_cookie1 = cookie,
                m_cookie2 = cookie,
                m_cookie3 = cookie,
                m_cookie4 = cookie
            }, 64*4);
            writer.FlushBit();

            var reader = new BitReader(writer.m_output);
            Assert.False(reader.ReadBit());
            Assert.Equal(cookie, reader.ReadBits<uint>(32));
            Assert.Equal(cookie, reader.ReadBits<ulong>(64));

            var cookieStruct = reader.ReadBits<CookieStruct>(64 * 4);
            Assert.Equal(cookie, cookieStruct.m_cookie1);
            Assert.Equal(cookie, cookieStruct.m_cookie2);
            Assert.Equal(cookie, cookieStruct.m_cookie3);
            Assert.Equal(cookie, cookieStruct.m_cookie4);
        }

        private struct CookieStruct
        {
            public ulong m_cookie1;
            public ulong m_cookie2;
            public ulong m_cookie3;
            public ulong m_cookie4;
        }

        [Fact]
        public void GrowableCanHandleWriteBits()
        {
            using var growable = new GrowingBitWriter();
            for (var i = 0; i < 1024; i++)
            {
                growable.WriteByte(0);
            }
            growable.WriteBits(0ul, 5);
            growable.GetData();
        }

        [Fact]
        public void ReadBits()
        {
            var reader = new BitReader(new byte[] {0xFF, 0xFF, 0xFF, 0xFF});
            Assert.Equal(0x7FFFFFFu, reader.ReadBits<uint>(27));
            Assert.Equal(27u, reader.m_fullBitOffset);
        }

        [Fact]
        public void WillNotGrowOnBoundary()
        {
            using var writer = new GrowingBitWriter();
            for (var i = 0; i < 1024-8; i++)
            {
                writer.WriteByte(0);
            }
            writer.WriteUInt64LittleEndian(0x1234DEADBEEF);
            Assert.Equal(1024, writer.m_memorySize);
        }
        
        [Fact]
        public void WillGrowOverBoundary()
        {
            using var writer = new GrowingBitWriter();
            for (var i = 0; i < 1024-7; i++)
            {
                writer.WriteByte(0);
            }
            writer.WriteUInt64LittleEndian(0x1234DEADBEEF);
            Assert.Equal(2048, writer.m_memorySize);
        }

        [Fact]
        public void AllTheTypes()
        {
            using var writer = new GrowingBitWriter();
            writer.WriteByte(123);
            writer.WriteSByte(-123);
            writer.WriteDoubleBigEndian(123);
            writer.WriteDoubleLittleEndian(123);
            writer.WriteInt16BigEndian(123);
            writer.WriteInt16LittleEndian(123);
            writer.WriteInt32BigEndian(123);
            writer.WriteInt32LittleEndian(123);
            writer.WriteInt64BigEndian(123);
            writer.WriteInt64LittleEndian(123);
            writer.WriteSingleBigEndian(123);
            writer.WriteSingleLittleEndian(123);
            writer.WriteUInt16BigEndian(123);
            writer.WriteUInt16LittleEndian(123);
            writer.WriteUInt32BigEndian(123);
            writer.WriteUInt32LittleEndian(123);
            writer.WriteUInt64BigEndian(123);
            writer.WriteUInt64LittleEndian(123);

            var reader = new BitReader(writer.GetData().Span);
            Assert.Equal(123, reader.ReadByte());
            Assert.Equal(-123, reader.ReadSByte());
            Assert.Equal(123, reader.ReadDoubleBigEndian());
            Assert.Equal(123, reader.ReadDoubleLittleEndian());
            Assert.Equal(123, reader.ReadInt16BigEndian());
            Assert.Equal(123, reader.ReadInt16LittleEndian());
            Assert.Equal(123, reader.ReadInt32BigEndian());
            Assert.Equal(123, reader.ReadInt32LittleEndian());
            Assert.Equal(123, reader.ReadInt64BigEndian());
            Assert.Equal(123, reader.ReadInt64LittleEndian());
            Assert.Equal(123, reader.ReadSingleBigEndian());
            Assert.Equal(123, reader.ReadSingleLittleEndian());
            Assert.Equal(123, reader.ReadUInt16BigEndian());
            Assert.Equal(123, reader.ReadUInt16LittleEndian());
            Assert.Equal(123u, reader.ReadUInt32BigEndian());
            Assert.Equal(123u, reader.ReadUInt32LittleEndian());
            Assert.Equal(123ul, reader.ReadUInt64BigEndian());
            Assert.Equal(123ul, reader.ReadUInt64LittleEndian());
            
            var reader2 = new ZeroTruncatedBitReader(writer.GetData().Span);
            Assert.Equal(123, reader2.ReadByte());
            Assert.Equal(-123, reader2.ReadSByte());
            Assert.Equal(123, reader2.ReadDoubleBigEndian());
            Assert.Equal(123, reader2.ReadDoubleLittleEndian());
            Assert.Equal(123, reader2.ReadInt16BigEndian());
            Assert.Equal(123, reader2.ReadInt16LittleEndian());
            Assert.Equal(123, reader2.ReadInt32BigEndian());
            Assert.Equal(123, reader2.ReadInt32LittleEndian());
            Assert.Equal(123, reader2.ReadInt64BigEndian());
            Assert.Equal(123, reader2.ReadInt64LittleEndian());
            Assert.Equal(123, reader2.ReadSingleBigEndian());
            Assert.Equal(123, reader2.ReadSingleLittleEndian());
            Assert.Equal(123, reader2.ReadUInt16BigEndian());
            Assert.Equal(123, reader2.ReadUInt16LittleEndian());
            Assert.Equal(123u, reader2.ReadUInt32BigEndian());
            Assert.Equal(123u, reader2.ReadUInt32LittleEndian());
            Assert.Equal(123ul, reader2.ReadUInt64BigEndian());
            Assert.Equal(123ul, reader2.ReadUInt64LittleEndian());
        }

        [Fact]
        public void ReaderSeek()
        {
            var reader = new BitReader(new byte[] {0xFF, 0xFF, 0xFF, 0xFF});
            
            Assert.Equal(uint.MaxValue, reader.ReadUInt32BigEndian());
            
            reader.SeekByte(0);
            Assert.Equal(uint.MaxValue, reader.ReadUInt32BigEndian());
            
            reader.SeekBit(0);
            Assert.Equal(uint.MaxValue, reader.ReadUInt32BigEndian());
            
            reader.SeekByte(1);
            Assert.Equal(1, reader.m_dataOffset);

            for (var i = 0u; i < 32; i++)
            {
                reader.SeekBit(i);
                Assert.Equal(i, reader.m_fullBitOffset);
            }
            
            reader.SeekBit(5);
            Assert.Equal(0x7FFFFFFu, reader.ReadBits<uint>(27));
            Assert.Equal(32u, reader.m_fullBitOffset);
        }
        
        [Fact]
        public void WriterSeek()
        {
            var writer = new BitWriter(new byte[4]);
            
            writer.SeekBit(0);
            writer.SeekBit(1);
            
            writer.SeekBit(0);
            writer.WriteBit(true);
            writer.SeekBit(0);
            writer.WriteBit(false);
            writer.FlushBit();
            Assert.Equal(0, writer.m_output[0]);
            Assert.Equal(0, writer.m_output[1]);
            Assert.Equal(0, writer.m_output[2]);
            Assert.Equal(0, writer.m_output[3]);
            
            writer.SeekBit(0);
            writer.WriteBit(true);
            writer.FlushBit(); // flush
            writer.SeekBit(0); // rewrind into flushed
            writer.WriteBit(false);
            writer.FlushBit();
            Assert.Equal(0, writer.m_output[0]);
            Assert.Equal(0, writer.m_output[1]);
            Assert.Equal(0, writer.m_output[2]);
            Assert.Equal(0, writer.m_output[3]);
            
            writer.SeekBit(0);
            writer.WriteBit(true);
            writer.FlushBit();
            Assert.Equal(1, writer.m_output[0]);
            Assert.Equal(0, writer.m_output[1]);
            Assert.Equal(0, writer.m_output[2]);
            Assert.Equal(0, writer.m_output[3]);
            
            
            writer.SeekBit(12);
            writer.WriteBit(true);
            
            writer.SeekByte(2);
            writer.WriteByte(0x12);

            var reader = new BitReader(writer.m_output);
            reader.SeekBit(11);
            Assert.False(reader.ReadBit());
            Assert.True(reader.ReadBit());
            
            reader.SeekByte(2);
            Assert.Equal(0x12, reader.ReadByte());
        }

        [Fact]
        public void ZeroNoData()
        {
            var reader = new ZeroTruncatedBitReader(ReadOnlySpan<byte>.Empty);
            Assert.False(reader.ReadBit());
            Assert.Equal(0ul, reader.ReadBits<ulong>(64));
            Assert.Equal(0, reader.ReadByte());
            Assert.Equal(0, reader.ReadSByte());
            Assert.Equal(0, reader.ReadDoubleBigEndian());
            Assert.Equal(0, reader.ReadDoubleLittleEndian());
            Assert.Equal(0, reader.ReadInt16BigEndian());
            Assert.Equal(0, reader.ReadInt16LittleEndian());
            Assert.Equal(0, reader.ReadInt32BigEndian());
            Assert.Equal(0, reader.ReadInt32LittleEndian());
            Assert.Equal(0, reader.ReadInt64BigEndian());
            Assert.Equal(0, reader.ReadInt64LittleEndian());
            Assert.Equal(0, reader.ReadSingleBigEndian());
            Assert.Equal(0, reader.ReadSingleLittleEndian());
            Assert.Equal(0, reader.ReadUInt16BigEndian());
            Assert.Equal(0, reader.ReadUInt16LittleEndian());
            Assert.Equal(0u, reader.ReadUInt32BigEndian());
            Assert.Equal(0u, reader.ReadUInt32LittleEndian());
            Assert.Equal(0ul, reader.ReadUInt64BigEndian());
            Assert.Equal(0ul, reader.ReadUInt64LittleEndian());
        }

        [Fact]
        public void ZeroReadLittleEndian()
        {
            var writer = new BitWriter(new byte[1]);
            writer.WriteByte(123);
            
            var reader = new ZeroTruncatedBitReader(writer.m_output);
            
            Assert.Equal(123, reader.ReadByte());
            
            reader.SeekByte(0);
            Assert.Equal(123, reader.ReadInt16LittleEndian());

            reader.SeekByte(0);
            Assert.Equal(123, reader.ReadInt32LittleEndian());
            
            reader.SeekByte(0);
            Assert.Equal(123, reader.ReadInt64LittleEndian());
            
            reader.SeekByte(0);
            Assert.Equal(123, reader.ReadUInt16LittleEndian());
            
            reader.SeekByte(0);
            Assert.Equal(123u, reader.ReadUInt32LittleEndian());
            
            reader.SeekByte(0);
            Assert.Equal(123ul, reader.ReadUInt64LittleEndian());

            for (var i = 8u; i < 64; i++)
            {
                reader.SeekByte(0);
                Assert.Equal(123ul, reader.ReadBits<ulong>(i));
            }
        }
    }
}