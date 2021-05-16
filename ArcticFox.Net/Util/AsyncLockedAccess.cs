using System;
using System.Threading;
using System.Threading.Tasks;

namespace ArcticFox.Net.Util
{
    public class AsyncLockedAccess<T>
    {
        private readonly SemaphoreSlim m_sema = new SemaphoreSlim(1, 1);
        private readonly AsyncLockToken<T> m_token;

        public AsyncLockedAccess(T value)
        {
            m_token = new AsyncLockToken<T>(m_sema, value);
        }

        public ValueTask<AsyncLockToken<T>> Get()
        {
            var waitTask = m_sema.WaitAsync();
            if (waitTask.IsCompleted) return new ValueTask<AsyncLockToken<T>>(m_token);
            return new ValueTask<AsyncLockToken<T>>(GetAwaited(waitTask)); // allocating path
        }
        
        public AsyncLockToken<T>? GetNoWait()
        {
            var result = m_sema.Wait(TimeSpan.Zero);
            if (result) return m_token;
            return null;
        }
        
        public AsyncLockToken<T> GetSync()
        {
            m_sema.Wait();
            return m_token;
        }

        private async Task<AsyncLockToken<T>> GetAwaited(Task waitTask)
        {
            await waitTask;
            return m_token;
        }
    }
}