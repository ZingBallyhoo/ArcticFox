using System.Threading;

namespace ArcticFox.Net.Util
{
    public class IDFactory
    {
        private long m_currId = 0;

        public ulong Next()
        {
            return (ulong)Interlocked.Increment(ref m_currId);
        }
    }
}