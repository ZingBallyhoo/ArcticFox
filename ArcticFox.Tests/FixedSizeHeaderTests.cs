using System;
using System.Buffers.Binary;
using ArcticFox.Net.Util;
using Xunit;

namespace ArcticFox.Tests
{
    public class FixedSizeHeaderTests
    {
        [Fact]
        public void TestA()
        {
            var header = new FixedSizeHeader(4);

            var buffer = new Memory<byte>(new byte[8]);
            var bufferSpan = buffer.Span;
            BinaryPrimitives.WriteUInt32BigEndian(bufferSpan, 0xDEADBEEF);
            BinaryPrimitives.WriteUInt32BigEndian(bufferSpan.Slice(4), 0xABCDEF);

            var bufferRO = (ReadOnlyMemory<byte>)buffer;
            Assert.True(header.ConsumeAndGet(ref bufferRO, out var headerData));
            
            Assert.Equal(0xDEADBEEF, BinaryPrimitives.ReadUInt32BigEndian(headerData.Span));
            Assert.Equal(0xABCDEFu, BinaryPrimitives.ReadUInt32BigEndian(bufferRO.Span));
        }
        
        [Fact]
        public void TestSplit()
        {
            var header = new FixedSizeHeader(4);

            var buffer = new Memory<byte>(new byte[4]);
            BinaryPrimitives.WriteUInt32BigEndian(buffer.Span, 0xDEADBEEF);
            
            var bufferRO = (ReadOnlyMemory<byte>)buffer;
            
            var bufferRO_1 = bufferRO.Slice(0, 1);
            Assert.False(header.Consume(ref bufferRO_1));
            
            var bufferRO_2 = bufferRO.Slice(1, 1);
            Assert.False(header.Consume(ref bufferRO_2));
            
            var bufferRO_3 = bufferRO.Slice(2, 2);
            Assert.True(header.ConsumeAndGet(ref bufferRO_3, out var headerData));
            
            Assert.Equal(0xDEADBEEF, BinaryPrimitives.ReadUInt32BigEndian(headerData.Span));
        }
    }
}