using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ArcticFox.Net.Batching;
using ArcticFox.Net.Sockets;

namespace ArcticFox.Net.Event
{
    public class NetEventQueue : IBroadcaster, IAsyncDisposable
    {
        private readonly SemaphoreSlim m_eventQueueSema = new SemaphoreSlim(1, 1);
        private readonly Queue<NetEvent> m_eventQueue = new Queue<NetEvent>();
        private bool m_acceptingEvents = true;
        
        public int m_maxQueueSize = 100;
        
        public void BroadcastEvent(NetEvent ev)
        {
            EnqueueEvent(ev);
        }

        private void EnqueueEvent(NetEvent @event)
        {
            m_eventQueueSema.Wait();
            try
            {
                if (!m_acceptingEvents) return;

                if (m_maxQueueSize != -1 && m_eventQueue.Count > m_maxQueueSize)
                {
                    // Log.Error("Send queue too big, dropping packet");
                    return;
                }
                @event.GetRef();
                m_eventQueue.Enqueue(@event);
            } finally
            {
                m_eventQueueSema.Release();
            }
        }

        public async Task<int> FlushEventsToSocket(SocketInterface socket, ISendContext ctx)
        {
            if (socket.IsClosed()) return 0;
            
            var gotLock = await m_eventQueueSema.WaitAsync(TimeSpan.Zero, default);
            if (!gotLock) return 0;

            int count;
            try
            {
                var q = m_eventQueue;
                count = q.Count;
                
                while (q.Count > 0)
                {                    
                    var ev = q.Dequeue();
                    await ctx.AddMessage(socket, ev.GetMemory(), q.Count);
                    ev.ReleaseRef();
                }
                await ctx.Flush(socket);
                
                return count;
            } finally
            {
                m_eventQueueSema.Release();
            }
        }
        
        private async Task DisposePendingEvents()
        {
            await m_eventQueueSema.WaitAsync();

            try
            {
                m_acceptingEvents = false;

                while (m_eventQueue.TryDequeue(out var ev))
                {
                    ev.ReleaseRef();
                }
            } finally
            {
                m_eventQueueSema.Release();
            }
        }
        
        public async ValueTask DisposeAsync()
        {
            await DisposePendingEvents();
        }
    }
}