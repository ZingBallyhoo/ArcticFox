using System;
using System.Threading;
using System.Threading.Tasks;

namespace ArcticFox.Net.Sockets
{
    public abstract class SocketInterface : IDisposable
    {
        public readonly CancellationTokenSource m_cancellationTokenSource;
        private bool m_closed;

        public SocketInterface()
        {
            m_cancellationTokenSource = new CancellationTokenSource();
        }
        
        public abstract ValueTask SendBuffer(ReadOnlyMemory<byte> data);

        public ValueTask SendBuffer(ReadOnlyMemory<byte> buffer, int offset, int length)
        {
            return SendBuffer(buffer.Slice(offset, length));
        }

        public abstract ValueTask<int> ReceiveBuffer(Memory<byte> buffer);

        public bool IsClosed() => m_closed;

        public void Close()
        {
            if (m_closed) return;
            m_closed = true;
            m_cancellationTokenSource.Cancel();
        }
        
        protected abstract ValueTask CloseSocket();

        public async ValueTask TryCloseSocket()
        {
            try
            {
                await CloseSocket();
            } catch (Exception)
            {
                // ignored
            }
        }

        public virtual void Dispose()
        {
            m_cancellationTokenSource.Dispose();
        }
    }
}