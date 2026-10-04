using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ArcticFox.Net.Sockets
{
    public class TcpSocket : SocketInterface
    {
        public readonly Socket m_socket;

        public TcpSocket(Socket socket)
        {
            m_socket = socket;
        }
        
        public override async ValueTask SendBuffer(ReadOnlyMemory<byte> memory)
        {
            await m_socket.SendAsync(memory, SocketFlags.None, m_cancellationTokenSource.Token);
        }

        public override ValueTask<int> ReceiveBuffer(Memory<byte> buffer)
        {
            return m_socket.ReceiveAsync(buffer, SocketFlags.None, m_cancellationTokenSource.Token);
        }

        protected override async ValueTask CloseSocket()
        {
            // not passing m_cancellationTokenSource.Token, because it's already canceled
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