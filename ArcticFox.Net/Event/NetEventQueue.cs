using System;
using System.Threading.Channels;
using System.Threading.Tasks;
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

        public async ValueTask FlushWorker(SocketInterface socket)
        {
            while (m_queue.Reader.CanPeek && !socket.IsClosed())
            {
                if (!m_queue.Reader.TryRead(out var message))
                {
                    await m_queue.Reader.WaitToReadAsync(socket.m_cancellationTokenSource.Token);
                    continue;
                }

                if (message is NetEvent netEvent)
                {
                    await socket.SendBuffer(netEvent.GetMemory());
                }
            }
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