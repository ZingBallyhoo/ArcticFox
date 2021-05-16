using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ArcticFox.Net.Sockets
{
    public class TcpServer : IDisposable
    {
        private readonly SocketHost m_host;
        private readonly IPEndPoint m_endPoint;
        private readonly Socket m_listenerSocket;
        
        public bool m_shutDown;
        
        public TcpServer(SocketHost host, IPEndPoint endPoint)
        {
            m_host = host;
            m_endPoint = endPoint;
            m_listenerSocket = new Socket(m_endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            m_host.GetCancellationToken().Register(() =>
            {
                m_listenerSocket.Dispose();
            });
            // lines above abort the listen on token cancel
            // https://github.com/dotnet/runtime/issues/33418 states AcceptAsync(token) was implemented in #40750? it wasn't
        }

        public void StartAcceptWorker()
        {
            Task.Run(AcceptWorker); // silent ignore
        }
        
        public async Task AcceptWorker()
        {
            m_listenerSocket.Bind(m_endPoint);  
            m_listenerSocket.Listen(100);

            try
            {
                while (m_host.IsRunning())
                {
                    var sockIt = await m_listenerSocket.AcceptAsync();
                    var tcpSocket = new TcpSocket(sockIt);
                    var highLevel = m_host.CreateHighLevelSocket(tcpSocket);
                    await m_host.AddSocket(highLevel);
                }
            } finally
            {
                m_shutDown = true;
            }
        }

        public void Dispose()
        {
            m_listenerSocket.Dispose();
        }
    }
}