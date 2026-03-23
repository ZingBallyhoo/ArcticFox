using System;
using System.Threading.Channels;
using System.Threading.Tasks;
using ArcticFox.Net.Batching;
using ArcticFox.Net.Sockets;

namespace ArcticFox.Net.Event
{
    public class NetEventQueue : IBroadcaster, IAsyncDisposable
    {
        private readonly Channel<object> m_queue;

        public NetEventQueue()
        {
            m_queue = Channel.CreateUnbounded<object>();
        }
        
        public ValueTask BroadcastEvent(NetEvent ev)
        {
            return EnqueueEvent(ev);
        }

        private async ValueTask EnqueueEvent(NetEvent @event)
        {
            if (m_queue.Writer.TryWrite(@event))
            {
                @event.GetRef();
            }
        }

        public async ValueTask FlushWorker(SocketInterface socket, ISendContext ctx)
        {
            var currentCount = 0;
            
            while (m_queue.Reader.CanPeek && !socket.IsClosed())
            {
                if (!m_queue.Reader.TryRead(out var message))
                {
                    await ctx.Flush(socket);
                    currentCount = 0;
                    
                    await m_queue.Reader.WaitToReadAsync(socket.m_cancellationTokenSource.Token);
                    continue;
                }

                if (message is NetEvent netEvent)
                {
                    await ctx.AddMessage(socket, netEvent.GetMemory(), currentCount);
                }
                
                currentCount++;
            }
            
            await ctx.Flush(socket);
        }
        
        private void DisposePendingEvents()
        {
            m_queue.Writer.Complete();
            
            while (m_queue.Reader.TryRead(out var message))
            {
                if (message is RefCounted refCounted)
                {
                    refCounted.ReleaseRef();
                }
            }
        }
        
        public ValueTask DisposeAsync()
        {
            DisposePendingEvents();
            return ValueTask.CompletedTask;
        }
    }
}