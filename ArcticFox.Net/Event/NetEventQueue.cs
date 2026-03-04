using System;
using System.Threading.Channels;
using System.Threading.Tasks;
using ArcticFox.Net.Batching;
using ArcticFox.Net.Sockets;
using Serilog;

namespace ArcticFox.Net.Event
{
    public class NetEventQueue : IBroadcaster, IAsyncDisposable
    {
        private readonly Channel<NetEvent> m_eventQueue;

        public NetEventQueue(int maxQueueSize = -1)
        {
            if (maxQueueSize >= 0)
            {
                m_eventQueue = Channel.CreateBounded<NetEvent>(maxQueueSize);
            } else
            {
                m_eventQueue = Channel.CreateUnbounded<NetEvent>();
            }
        }
        
        public ValueTask BroadcastEvent(NetEvent ev)
        {
            return EnqueueEvent(ev);
        }

        private async ValueTask EnqueueEvent(NetEvent @event)
        {
            @event.GetRef();
            if (!m_eventQueue.Writer.TryWrite(@event))
            {
                @event.ReleaseRef();
                
                // todo: gross.
                // the socket needs to be closed or something
                Log.Error("Send queue too big, dropping packet");
            }
        }

        public async ValueTask FlushWorker(SocketInterface socket, ISendContext ctx)
        {
            var currentCount = 0;
            
            while (m_eventQueue.Reader.CanPeek && !socket.IsClosed())
            {
                if (!m_eventQueue.Reader.TryRead(out var ev))
                {
                    await ctx.Flush(socket);
                    currentCount = 0;
                    
                    await m_eventQueue.Reader.WaitToReadAsync(socket.m_cancellationTokenSource.Token);
                    continue;
                }
                
                await ctx.AddMessage(socket, ev.GetMemory(), currentCount);
                ev.ReleaseRef();

                currentCount++;
            }
            
            await ctx.Flush(socket);
        }
        
        private void DisposePendingEvents()
        {
            m_eventQueue.Writer.Complete();
            
            while (m_eventQueue.Reader.TryRead(out var ev))
            {
                ev.ReleaseRef();
            }
        }
        
        public ValueTask DisposeAsync()
        {
            DisposePendingEvents();
            return ValueTask.CompletedTask;
        }
    }
}