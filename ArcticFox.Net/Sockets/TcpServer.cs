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
        }

        public void StartAcceptWorker()
        {
            Task.Run(AcceptWorker); // silent ignore
        }
        
        public async Task AcceptWorker()
        {
            var cancellationToken = m_host.GetCancellationToken();

            m_listenerSocket.Bind(m_endPoint);  
            m_listenerSocket.Listen(100);

            try
            {
                while (m_host.IsRunning())
                {
                    var sockIt = await m_listenerSocket.AcceptAsync(cancellationToken);
                    var tcpSocket = new TcpSocket(sockIt);
                    var highLevel = m_host.CreateHighLevelSocket(tcpSocket);
                    await m_host.AddSocket(highLevel);
                }
            } finally
            {
                m_listenerSocket.Dispose();
                m_shutDown = true;
            }
        }

        public void Dispose()
        {
            m_listenerSocket.Dispose();
        }
    }
}