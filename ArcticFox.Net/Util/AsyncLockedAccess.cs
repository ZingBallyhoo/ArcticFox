using System.Threading;
using System.Threading.Tasks;

namespace ArcticFox.Net.Util
{
    public class AsyncLockedAccess<T>
    {
        private readonly SemaphoreSlim m_sema = new SemaphoreSlim(1, 1);
        private readonly T m_value;

        public AsyncLockedAccess(T value)
        {
            m_value = value;
        }

        public async Task<AsyncLockToken<T>> Get()
        {
            await m_sema.WaitAsync();
            return new AsyncLockToken<T>(m_sema, m_value);
        }
    }
}