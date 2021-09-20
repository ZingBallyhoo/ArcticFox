using System;
using System.Buffers.Binary;
using ArcticFox.Codec;
using Xunit;

namespace ArcticFox.Tests
{
    public class FixedSizeHeaderTests
    {
        [Fact]
        public void TestA()
        {
            var header = new FixedSizeHeader<byte>(4);

            var buffer = new Span<byte>(new byte[8]);
            BinaryPrimitives.WriteUInt32BigEndian(buffer, 0xDEADBEEF);
            BinaryPrimitives.WriteUInt32BigEndian(buffer.Slice(4), 0xABCDEF);

            var bufferRO = (ReadOnlySpan<byte>)buffer;
            Assert.True(header.ConsumeAndGet(ref bufferRO, out var headerData));
            
            Assert.Equal(0xDEADBEEF, BinaryPrimitives.ReadUInt32BigEndian(headerData));
            Assert.Equal(0xABCDEFu, BinaryPrimitives.ReadUInt32BigEndian(bufferRO));
        }
        
        [Fact]
        public void TestSplit()
        {
            var header = new FixedSizeHeader<byte>(4);

            var buffer = new Span<byte>(new byte[4]);
            BinaryPrimitives.WriteUInt32BigEndian(buffer, 0xDEADBEEF);
            
            var bufferRO = (ReadOnlySpan<byte>)buffer;
            
            var bufferRO_1 = bufferRO.Slice(0, 1);
            Assert.False(header.Consume(ref bufferRO_1));
            
            var bufferRO_2 = bufferRO.Slice(1, 1);
            Assert.False(header.Consume(ref bufferRO_2));
            
            var bufferRO_3 = bufferRO.Slice(2, 2);
            Assert.True(header.ConsumeAndGet(ref bufferRO_3, out var headerData));
            
            Assert.Equal(0xDEADBEEF, BinaryPrimitives.ReadUInt32BigEndian(headerData));
        }
    }
}