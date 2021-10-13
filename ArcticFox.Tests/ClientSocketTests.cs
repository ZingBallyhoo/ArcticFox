using System;
using System.Net;
using System.Threading.Tasks;
using ArcticFox.Codec;
using ArcticFox.Net;
using ArcticFox.Net.Sockets;
using ArcticFox.Tests.Impls;
using Xunit;

namespace ArcticFox.Tests
{
    public class EchoSocket : HighLevelSocket, ISpanConsumer<byte>
    {
        public EchoSocket(SocketInterface socket) : base(socket)
        {
            m_netInputCodec = new CodecChain().AddCodec(this);
        }

        public void Input(ReadOnlySpan<byte> input, ref object? state)
        {
            // its ok to pass the span, method runs synchronously
            TempBroadcasterExtensions.BroadcastBytes(this, input).GetAwaiter().GetResult();
        }

        public void Abort()
        {
            Close();
        }
    }

    public class EchoSocketHost : SocketHost
    {
        public override HighLevelSocket CreateHighLevelSocket(SocketInterface socket)
        {
            return new EchoSocket(socket);
        }
    }
    
    public class ClientSocketTests
    {
        [Fact]
        public async Task TestEchoWS()
        {
            await using var host = new TestSocketHost();
            await host.StartAsync();

            await using var socket = await host.CreateClientWebSocket<TestSocket>(new Uri("wss://echo.websocket.org"));

            await socket.BroadcastZeroTerminatedAscii("Hello Echo Bois\0"); // todo: can't send more than 1 \0? it doesn't work
            await Task.Delay(500); // this is over the internet lol

            Assert.Equal(new []{"Hello Echo Bois"}, socket.m_received);
        }
        
        [Fact]
        public async Task TestEchoTcpInProcess()
        {
            await using var clientHost = new TestSocketHost();
            await clientHost.StartAsync();

            var endPoint = IPEndPoint.Parse("127.0.0.1:9002");
            await using var serverHost = new EchoSocketHost();
            await serverHost.StartAsync();
            using var server = new TcpServer(serverHost, endPoint);
            server.StartAcceptWorker();

            await using var socket = await clientHost.CreateClientTcpSocket<TestSocket>(endPoint);
            await socket.BroadcastZeroTerminatedAscii("Hello Echo Bois\0");
            
            await Task.Delay(50);
            Assert.Equal(1, await serverHost.GetSocketCount());
            Assert.Equal(new []{"Hello Echo Bois"}, socket.m_received);
            
            // sanity, cts doesn't break
            socket.Close();
            socket.Close();
            socket.Close();
            socket.Close();
        }
    }
}