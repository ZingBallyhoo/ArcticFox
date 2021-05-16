using System.Threading;
using System.Threading.Tasks;

namespace ArcticFox.Net.Sockets
{
    public abstract class CancellingSocket : SocketInterface
    {
        public readonly CancellationTokenSource m_cancellationTokenSource;
        
        public CancellingSocket()
        {
            m_cancellationTokenSource = new CancellationTokenSource();
        }

        protected override Task CloseSocket()
        {
            m_cancellationTokenSource.Cancel();
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            base.Dispose();
            m_cancellationTokenSource.Dispose();
        }
    }
}