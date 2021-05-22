using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArcticFox.Net.Batching;
using ArcticFox.Net.Sockets;
using ArcticFox.Net.Util;

namespace ArcticFox.Net.Event
{
    public class NetEventQueue : IBroadcaster, IAsyncDisposable
    {
        private readonly AsyncLockedAccess<Queue<NetEvent>> m_eventQueue = new AsyncLockedAccess<Queue<NetEvent>>(new Queue<NetEvent>());
        private bool m_acceptingEvents = true;
        
        public int m_maxQueueSize = 100;
        
        public ValueTask BroadcastEvent(NetEvent ev)
        {
            return EnqueueEvent(ev);
        }

        private async ValueTask EnqueueEvent(NetEvent @event)
        {
            using (var queueToken = await m_eventQueue.Get())
            {
                var queue = queueToken.m_value;
                if (!m_acceptingEvents) return;

                if (m_maxQueueSize != -1 && queue.Count > m_maxQueueSize)
                {
                    // Log.Error("Send queue too big, dropping packet");
                    return;
                }
                @event.GetRef();
                queue.Enqueue(@event);
            }
        }

        public async ValueTask<int> FlushEventsToSocket(SocketInterface socket, ISendContext ctx)
        {
            if (socket.IsClosed()) return 0;
            
            using (var queueToken = m_eventQueue.GetNoWait())
            {
                if (queueToken == null) return 0;
                
                var queue = queueToken.m_value;
                var count = queue.Count;
                
                while (queue.Count > 0)
                {                    
                    var ev = queue.Dequeue();
                    await ctx.AddMessage(socket, ev.GetMemory(), queue.Count);
                    ev.ReleaseRef();
                }
                await ctx.Flush(socket);
                
                return count;
            }
        }
        
        private async ValueTask DisposePendingEvents()
        {
            using (var token = await m_eventQueue.Get())
            {
                m_acceptingEvents = false;

                while (token.m_value.TryDequeue(out var ev))
                {
                    ev.ReleaseRef();
                }
            }
        }
        
        public async ValueTask DisposeAsync()
        {
            await DisposePendingEvents();
        }
    }
}