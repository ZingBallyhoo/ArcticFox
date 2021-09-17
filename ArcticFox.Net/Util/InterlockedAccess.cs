using System;
using System.Threading;
using System.Threading.Tasks;

namespace ArcticFox.Net.Util
{
    public class InterlockedAccess<T>
    {
        private readonly Token m_token;
        private readonly Task<Token> m_tokenTask;
        
        private int m_state;

        private const int STATE_FREE = 0;
        private const int STATE_IN_USE = 1;

        public InterlockedAccess(T value)
        {
            m_token = new Token(value, this);
            m_tokenTask = Task.FromResult(m_token);
        }

        public Task<Token> Get()
        {
            if (Interlocked.CompareExchange(ref m_state, STATE_IN_USE, STATE_FREE) == STATE_FREE)
            {
                return m_tokenTask;
            }
            return GetAsync();
        }

        public Token? TryGet()
        {
            if (Interlocked.CompareExchange(ref m_state, STATE_IN_USE, STATE_FREE) == STATE_FREE)
            {
                return m_token;
            }
            return null;
        }

        private async Task<Token> GetAsync()
        {
            while (Interlocked.CompareExchange(ref m_state, STATE_IN_USE, STATE_FREE) != STATE_FREE)
            {
                await Task.Yield();
            }
            return m_token;
        }

        private void Release()
        {
            Interlocked.Exchange(ref m_state, 0);
        }
        
        public class Token : IDisposable
        {
            public readonly T m_value;
            private readonly InterlockedAccess<T> m_owner;

            public Token(T value, InterlockedAccess<T> owner)
            {
                m_value = value;
                m_owner = owner;
            }

            public void Dispose()
            {
                m_owner.Release();
            }
        }
    }
}