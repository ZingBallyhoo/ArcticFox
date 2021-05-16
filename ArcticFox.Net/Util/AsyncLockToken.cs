using System;
using System.Threading;

namespace ArcticFox.Net.Util
{
    public class AsyncLockToken<T> : IDisposable
    {
        private readonly SemaphoreSlim m_sema;
        public T m_value;
        
        public AsyncLockToken(SemaphoreSlim sema, T value)
        {
            m_sema = sema;
            m_value = value;
        }

        public void Dispose()
        {
            m_sema.Release();
        }
    }
}