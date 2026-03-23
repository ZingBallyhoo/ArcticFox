using System;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ArcticFox.Net.Util
{
    public class TaskQueue
    {
        private readonly Channel<Func<ValueTask>> m_channel;
        
        public TaskQueue()
        {
            m_channel = Channel.CreateUnbounded<Func<ValueTask>>();
        }

        public void Enqueue(Func<ValueTask> taskGenerator)
        {
            m_channel.Writer.TryWrite(taskGenerator);
        }
        
        public async ValueTask ConsumeAll()
        {
            while (m_channel.Reader.TryRead(out var taskFactory))
            {
                await taskFactory();
            }
        }

        public void Complete()
        {
            m_channel.Writer.TryComplete();
        }
    }
}