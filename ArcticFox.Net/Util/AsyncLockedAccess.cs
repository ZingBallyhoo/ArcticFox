using System;
using System.Threading;
using System.Threading.Tasks;

namespace ArcticFox.Net.Util
{
    public class AsyncLockedAccess<T>
    {
        private readonly SemaphoreSlim m_sema;
        private readonly Token m_token;
        private readonly Task<Token> m_tokenTask;

        public AsyncLockedAccess(T value)
        {
            m_sema = new SemaphoreSlim(1, 1);
            m_token = new Token(this, value);
            m_tokenTask = Task.FromResult(m_token);
        }

        public Task<Token> Get()
        {
            var task = m_sema.WaitAsync();
            return task.IsCompleted ? m_tokenTask : GetAsync(task);
        }
        
        private async Task<Token> GetAsync(Task task)
        {
            await task.ConfigureAwait(false);
            return m_token;
        }
        
        public Token? TryGet()
        {
            var result = m_sema.Wait(TimeSpan.Zero);
            return result ? m_token : null;
        }
        
        public Token GetSync()
        {
            m_sema.Wait();
            return m_token;
        }

        private void Release()
        {
            m_sema.Release();
        }
        
        public class Token : IDisposable
        {
            private readonly AsyncLockedAccess<T> m_owner;
            public T m_value;
        
            public Token(AsyncLockedAccess<T> owner, T value)
            {
                m_owner = owner;
                m_value = value;
            }

            public void Dispose()
            {
                m_owner.Release();
            }
        }
    }
}