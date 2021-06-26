using ArcticFox.Codec.Binary;
using Xunit;

namespace ArcticFox.Tests
{
    public class BinaryTests
    {
        [Fact]
        public void GrowableBufferHandlesDanglingByte()
        {
            using var growableBuffer = new GrowingBitWriter(0);
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
        public void GrowableCanHandleWriteBits()
        {
            using var growable = new GrowingBitWriter(0);
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
            using var writer = new GrowingBitWriter(0);
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
            using var writer = new GrowingBitWriter(0);
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
            using var writer = new GrowingBitWriter(0);
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
        }
    }
}