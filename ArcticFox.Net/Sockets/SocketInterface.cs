using System;
using System.Threading.Tasks;

namespace ArcticFox.Net.Sockets
{
    public abstract class SocketInterface : IDisposable
    {
        private bool m_closed;
        
        public abstract Task SendBuffer(ReadOnlyMemory<byte> data);

        public Task SendBuffer(ReadOnlyMemory<byte> buffer, int offset, int length)
        {
            return SendBuffer(buffer.Slice(offset, length));
        }

        public abstract Task<int> ReceiveBuffer(Memory<byte> buffer);

        public bool IsClosed() => m_closed;

        public void Close()
        {
            m_closed = true;
        }
        
        protected abstract Task CloseSocket();

        public async Task TryCloseSocket()
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
        }
    }
}