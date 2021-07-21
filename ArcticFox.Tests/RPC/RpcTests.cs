using System;
using System.Net;
using System.Threading.Tasks;
using ArcticFox.Net;
using ArcticFox.Net.Sockets;
using Xunit;

namespace ArcticFox.Tests.RPC
{
    public class RpcClientSocketHost : SocketHost
    {
        public RpcClientSocketHost()
        {
            m_batchMessages = false;
        }
        
        public override HighLevelSocket CreateHighLevelSocket(SocketInterface socket)
        {
            return new MyRpcClientSocket(socket);
        }
    }
    
    public class RpcServerSocketHost : SocketHost
    {
        public RpcServerSocketHost()
        {
            m_batchMessages = false;
        }
        
        public override HighLevelSocket CreateHighLevelSocket(SocketInterface socket)
        {
            return new MyRpcServerSocket(socket);
        }
    }
    
    public class RpcTests
    {
        [Fact]
        public async Task a()
        {
            var endPoint = IPEndPoint.Parse("127.0.0.1:9005");
            
            await using var clientHost = new RpcClientSocketHost();
            await clientHost.StartAsync();
            
            await using var host = new RpcServerSocketHost();
            await host.StartAsync();
            var server = new TcpServer(host, endPoint);
            server.StartAcceptWorker();

            var remoteService = new MyService_Remote();
            
            await using var socket = await clientHost.CreateClientTCPSocket<MyRpcClientSocket>(endPoint);
            
            // normal request
            var response1 = await remoteService.One(socket, new Request1());
            Assert.NotNull(response1);
            
            // spam
            await remoteService.Two(socket, new Request2());
            await remoteService.Two(socket, new Request2());
            var response2 = await remoteService.Two(socket, new Request2());
            await remoteService.Two(socket, new Request2());
            await remoteService.Two(socket, new Request2());
            await remoteService.Two(socket, new Request2());
            Assert.NotNull(response2);
            
            // fire and forget
            await MyService.OneMethod.CallAsync(socket, new Request1());
            await MyService.ThreeMethod.CallAsync(socket, new Request3());
            
            // invoke via definition
            var response1ViaMember = await MyService.OneMethod.Call(socket, new Request1());
            Assert.NotNull(response1ViaMember);

            // with callback function
            var tcs = new TaskCompletionSource();
            await MyService.OneMethod.CallAsync(socket, new Request1(), responseAsync =>
            {
                Assert.NotNull(responseAsync);
                tcs.SetResult();
                return ValueTask.CompletedTask;
            });
            await tcs.Task.WaitAsync(TimeSpan.FromSeconds(1));

            // exception handling
            await Assert.ThrowsAsync<Exception>(async () =>
            {
                await remoteService.Four(socket, new Request4());
            });
        }

        [Fact]
        public void TokenBasedDoesNotAllowNull()
        {
            Assert.Throws<ArgumentNullException>(() => MyService.OneMethod.DecodeRequest(ReadOnlySpan<byte>.Empty, null));
            Assert.Throws<ArgumentNullException>(() => MyService.TwoMethod.DecodeRequest(ReadOnlySpan<byte>.Empty, null));
            Assert.Throws<ArgumentNullException>(() => MyService.ThreeMethod.DecodeRequest(ReadOnlySpan<byte>.Empty, null));
            Assert.Throws<ArgumentNullException>(() => MyService.FourMethod.DecodeRequest(ReadOnlySpan<byte>.Empty, null));
            
            Assert.Throws<ArgumentNullException>(() => MyService.OneMethod.DecodeResponse(ReadOnlySpan<byte>.Empty, null));
            Assert.Throws<ArgumentNullException>(() => MyService.TwoMethod.DecodeResponse(ReadOnlySpan<byte>.Empty, null));
        }
    }
}