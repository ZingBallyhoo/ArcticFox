using System;
using System.Threading;
using System.Threading.Tasks;

namespace ArcticFox.Net.Util
{
    public class AsyncLockedAccess<T>
    {
        private readonly SemaphoreSlim m_sema;
        private readonly AsyncLockToken<T> m_token;
        private readonly Task<AsyncLockToken<T>> m_tokenTask;

        public AsyncLockedAccess(T value)
        {
            m_sema = new SemaphoreSlim(1, 1);
            m_token = new AsyncLockToken<T>(m_sema, value);
            m_tokenTask = Task.FromResult(m_token);
        }

        public Task<AsyncLockToken<T>> Get()
        {
            var task = m_sema.WaitAsync();
            return task.IsCompleted ? m_tokenTask : GetAwaited(task);
        }
        
        private async Task<AsyncLockToken<T>> GetAwaited(Task task)
        {
            await task.ConfigureAwait(false);
            return m_token;
        }
        
        public AsyncLockToken<T>? GetNoWait()
        {
            var result = m_sema.Wait(TimeSpan.Zero);
            return result ? m_token : null;
        }
        
        public AsyncLockToken<T> GetSync()
        {
            m_sema.Wait();
            return m_token;
        }
    }
}