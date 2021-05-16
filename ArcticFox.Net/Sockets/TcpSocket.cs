using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ArcticFox.Net.Sockets
{
    public class TcpSocket : CancellingSocket
    {
        public readonly Socket m_socket;

        public TcpSocket(Socket socket)
        {
            m_socket = socket;
        }
        
        public override async Task SendBuffer(ReadOnlyMemory<byte> memory)
        {
            await m_socket.SendAsync(memory, SocketFlags.None);
        }

        public override async Task<int> ReceiveBuffer(Memory<byte> buffer)
        {
            return await m_socket.ReceiveAsync(buffer, SocketFlags.None, m_cancellationTokenSource.Token);
        }

        protected override async Task CloseSocket()
        {
            await base.CloseSocket();
            await m_socket.DisconnectAsync(false);
            m_socket.Close();
        }

        public override void Dispose()
        {
            base.Dispose();
            m_socket.Dispose();
        }
    }
}