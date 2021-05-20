using System;
using System.Threading;
using System.Threading.Tasks;

namespace ArcticFox.Net.Util
{
    public class TaskQueue
    {
        private readonly SemaphoreSlim m_sema;
        
        public TaskQueue()
        {
            m_sema = new SemaphoreSlim(1, 1);
        }

        public async Task<T> Enqueue<T>(Func<Task<T>> taskGenerator)
        {
            await m_sema.WaitAsync();
            try
            {
                return await taskGenerator();
            }
            finally
            {
                m_sema.Release();
            }
        }
        
        public async Task Enqueue(Func<Task> taskGenerator)
        {
            await m_sema.WaitAsync();
            try
            {
                await taskGenerator();
            }
            finally
            {
                m_sema.Release();
            }
        }
    }
}