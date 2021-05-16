using System;
using System.Threading.Tasks;
using ArcticFox.Net.Batching;
using ArcticFox.Tests.Impls;
using Xunit;

namespace ArcticFox.Tests
{
    public class BatchedSendContextTests
    {
        [Fact]
        public async Task TestSharedBuffer()
        {
            using var socket = new TestSocketInterface();
            var ctx = new BatchedSendContext(new Memory<byte>(new byte[2]));

            await ctx.AddMessage(socket, new ReadOnlyMemory<byte>(new[] {(byte)'A'}), 3);
            await ctx.AddMessage(socket, new ReadOnlyMemory<byte>(new[] {(byte)'B'}), 2);
            await ctx.AddMessage(socket, new ReadOnlyMemory<byte>(new[] {(byte)'C'}), 1);
            await ctx.AddMessage(socket, new ReadOnlyMemory<byte>(new[] {(byte)'D'}), 0);
            await ctx.Flush(socket);

            socket.AssertSent("AB", "CD");
        }
        
        [Fact]
        public async Task TestFlush()
        {
            using var socket = new TestSocketInterface();
            var ctx = new BatchedSendContext(new Memory<byte>(new byte[10]));

            await ctx.AddMessage(socket, new ReadOnlyMemory<byte>(new[] {(byte)'A', (byte)'B'}), 1);
            await ctx.AddMessage(socket, new ReadOnlyMemory<byte>(new[] {(byte)'C'}), 0);
            await ctx.Flush(socket);
            
            socket.AssertSent("ABC");
        }
        
        [Fact]
        public async Task TestExistingBuffer()
        {
            using var socket = new TestSocketInterface();
            var ctx = new BatchedSendContext(new Memory<byte>(new byte[2]));

            await ctx.AddMessage(socket, new ReadOnlyMemory<byte>(new[] {(byte)'A', (byte)'B', (byte)'C', (byte)'D'}), 1);
            await ctx.AddMessage(socket, new ReadOnlyMemory<byte>(new[] {(byte)'A', (byte)'B', (byte)'C', (byte)'D'}), 0);
            await ctx.Flush(socket);

            socket.AssertSent("AB", "CD", "AB", "CD");
        }
    }
}