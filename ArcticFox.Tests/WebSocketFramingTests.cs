using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using ArcticFox.Codec;
using ArcticFox.Net.Batching;
using ArcticFox.Net.Sockets;
using ArcticFox.Tests.Impls;
using Xunit;

namespace ArcticFox.Tests
{
    
    public class TestWebSocketInterface : WebSocketInterface
    {
        public List<string> m_sentAsStrings = new List<string>();
        public const string c_endOfMessageString = "_ENDOFMESSAGE";

        public TestWebSocketInterface() : base(new ClientWebSocket(), true)
        { }
        
        public override ValueTask SendBuffer(ReadOnlyMemory<byte> data)
        {
            throw new NotImplementedException();
        }

        public override ValueTask SendBuffer(ReadOnlyMemory<byte> data, bool endOfMessage)
        {
            var text = Encoding.ASCII.GetString(data.Span);
            if (endOfMessage) text += c_endOfMessageString;
            m_sentAsStrings.Add(text);
            return ValueTask.CompletedTask;
        }

        public override ValueTask<int> ReceiveBuffer(Memory<byte> buffer)
        {
            throw new NotImplementedException();
        }

        protected override ValueTask CloseSocket()
        {
            throw new NotImplementedException();
        }
        
        public void AssertSent(params string[] expected)
        {
            Assert.Equal(expected, m_sentAsStrings);
            m_sentAsStrings.Clear();
        }
    }
    
    public class TestInputWebSocketInterface : WebSocketInterface
    {
        public TestInputWebSocketInterface() : base(new ClientWebSocket(), true)
        { }
        
        public override ValueTask SendBuffer(ReadOnlyMemory<byte> data)
        {
            throw new NotImplementedException();
        }

        public override ValueTask SendBuffer(ReadOnlyMemory<byte> data, bool endOfMessage)
        {
            throw new NotImplementedException();
        }

        public override async ValueTask<int> ReceiveBuffer(Memory<byte> buffer)
        {
            throw new NotImplementedException();
        }

        protected override ValueTask CloseSocket()
        {
            throw new NotImplementedException();
        }

        public void SetEndOfMessage(bool eom)
        {
            m_lastRecvWasEndOfMessage = eom;
        }
    }
    
    public class WebSocketFramingTests
    {
        [Fact]
        public async Task TestSend()
        {
            using var socket = new TestWebSocketInterface();
            var ctx = new WebSocketAwareSendContext(2);

            await ctx.AddMessage(socket, new ReadOnlyMemory<byte>(new[] {(byte)'A', (byte)'B', (byte)'C'}), 2);
            await ctx.AddMessage(socket, new ReadOnlyMemory<byte>(new[] {(byte)'D'}), 1);
            await ctx.AddMessage(socket, new ReadOnlyMemory<byte>(new[] {(byte)'E', (byte)'F'}), 0);
            await ctx.Flush(socket);

            var eom = TestWebSocketInterface.c_endOfMessageString;
            socket.AssertSent("AB", $"C{eom}", $"D{eom}", $"EF{eom}");
        }
        
        [Fact]
        public void TestReceive()
        {
            var output = new TestEncodeOutputCodec();
            
            using var chain = new CodecChain();
            chain.AddCodec(new WebSocketFramingInputCodec(1000));
            chain.AddCodec(output);

            var fakeWs = new TestInputWebSocketInterface();
            
            fakeWs.SetEndOfMessage(true);
            chain.Head<byte>().Input2(new ReadOnlySpan<byte>(new[] {(byte)'A', (byte)'B', (byte)'C'}), fakeWs);
            output.AssertOutput("ABC");
            

            fakeWs.SetEndOfMessage(false);
            chain.Head<byte>().Input2(new ReadOnlySpan<byte>(new[] {(byte)'A', (byte)'B', (byte)'C'}), fakeWs);
            output.AssertOutput();
            
            fakeWs.SetEndOfMessage(true);
            chain.Head<byte>().Input2(new ReadOnlySpan<byte>(new[] {(byte)'D'}), fakeWs);
            output.AssertOutput("ABCD");
        }
        
        [Fact]
        public void TestReceiveTooBig()
        {
            var output = new TestEncodeOutputCodec();
            
            using var chain = new CodecChain();
            chain.AddCodec(new WebSocketFramingInputCodec(2));
            chain.AddCodec(output);

            var fakeWs = new TestInputWebSocketInterface();
            
            fakeWs.SetEndOfMessage(false);
            chain.Head<byte>().Input2(new ReadOnlySpan<byte>(new[] {(byte)'A'}), fakeWs);
            fakeWs.SetEndOfMessage(true);
            chain.Head<byte>().Input2(new ReadOnlySpan<byte>(new[] {(byte)'B'}), fakeWs);
            output.AssertOutput("AB");
            
            fakeWs.SetEndOfMessage(false);
            chain.Head<byte>().Input2(new ReadOnlySpan<byte>(new[] {(byte)'A'}), fakeWs);
            fakeWs.SetEndOfMessage(true);
            Assert.Throws<InvalidDataException>(() =>
            {
                chain.Head<byte>().Input2(new ReadOnlySpan<byte>(new[] {(byte) 'B', (byte) 'C'}), fakeWs);
            });
        }
    }
}