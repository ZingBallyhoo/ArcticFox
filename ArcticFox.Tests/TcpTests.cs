using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ArcticFox.Net.Sockets;
using ArcticFox.Tests.Impls;
using Xunit;

namespace ArcticFox.Tests
{
    public class TcpTests
    {
        [Fact]
        public async Task Test()
        {
            var endPoint = IPEndPoint.Parse("127.0.0.1:9003");
            
            await using var host = new TestSocketHost();
            await host.StartAsync();
            var server = new TcpServer(host, endPoint);
            server.StartAcceptWorker();

            using var client = new TcpClient();
            await client.ConnectAsync(endPoint);
            using var client2 = new TcpClient();
            await client2.ConnectAsync(endPoint);

            await Task.Delay(50);
            Assert.Equal(2, await host.GetSocketCount());
            
            client2.Client.Shutdown(SocketShutdown.Both);
            await client2.Client.DisconnectAsync(false);
            client2.Close();
            client2.Dispose();
            
            await Task.Delay(400); // timing is hmmm
            Assert.Equal(1, await host.GetSocketCount());
            
            using var client1Stream = client.GetStream();
            await client1Stream.WriteAsync(Encoding.ASCII.GetBytes("Hello\0World"));
            await Task.Delay(40);

            var serverSideSocket = (TestSocket)(await host.GetSockets())[0];
            Assert.Equal(serverSideSocket.m_received, new []{"Hello"});
            
            await client1Stream.WriteAsync(Encoding.ASCII.GetBytes("\0"));
            await Task.Delay(20);
            
            Assert.Equal(serverSideSocket.m_received, new []{"Hello", "World"});

            await host.StopAsync();
            Assert.Equal(0, await host.GetSocketCount());
            
            await Task.Delay(40); // timing is hmmm
            Assert.True(server.m_shutDown);
        }
        
        [Theory]
        [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(4)] [InlineData(5)]
        [InlineData(6)] [InlineData(7)] [InlineData(8)] [InlineData(9)] [InlineData(10)]
        public async Task TestTooSmallRecvBuffer(int size)
        {
            var endPoint = IPEndPoint.Parse("127.0.0.1:9004");
            
            await using var host = new TestSocketHost();
            host.m_recvBufferSize = size;
            await host.StartAsync();
            
            using var server = new TcpServer(host, endPoint);
            server.StartAcceptWorker();

            using var client = new TcpClient();
            await client.ConnectAsync(endPoint);
            using var clientStream = client.GetStream();
            await clientStream.WriteAsync(Encoding.ASCII.GetBytes("Hello\0World\0"));
            await Task.Delay(50);
            
            var serverSideSocket = (TestSocket)(await host.GetSockets())[0];
            Assert.Equal(new []{"Hello", "World"}, serverSideSocket.m_received);
        }
    }
}