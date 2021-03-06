using System;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading.Tasks;
using ArcticFox.Net.Sockets;

namespace ArcticFox.Net
{
    public static class ClientSocketHostExtensions
    {
        public static async ValueTask<T> CreateClientWebSocket<T>(this SocketHost host, Uri uri, bool binaryMessages=true) where T : HighLevelSocket
        {
            var clientWebSocket = new ClientWebSocket();
            await clientWebSocket.ConnectAsync(uri, default);
            
            var webSocket = new WebSocketInterface(clientWebSocket, binaryMessages);
            var hl = host.CreateHighLevelSocket(webSocket);
            await host.AddSocket(hl); // must be after connect otherwise recv loop instantly errors
            return (T)hl;
        }
        
        public static async ValueTask<T> CreateClientTcpSocket<T>(this SocketHost host, EndPoint endPoint) where T : HighLevelSocket
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await socket.ConnectAsync(endPoint);
            
            var tcpSocket = new TcpSocket(socket);
            var hl = host.CreateHighLevelSocket(tcpSocket);
            await host.AddSocket(hl);
            return (T)hl;
        }
        
        public static async ValueTask<T> CreateClientSslSocket<T>(this SocketHost host, DnsEndPoint endPoint) where T : HighLevelSocket
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await socket.ConnectAsync(endPoint);

            var netStream = new NetworkStream(socket);
            var sslStream = new SslStream(netStream);
            await sslStream.AuthenticateAsClientAsync(endPoint.Host);

            var tcpSocket = new StreamSocket(sslStream);
            var hl = host.CreateHighLevelSocket(tcpSocket);
            await host.AddSocket(hl);
            return (T)hl;
        }
    }
}