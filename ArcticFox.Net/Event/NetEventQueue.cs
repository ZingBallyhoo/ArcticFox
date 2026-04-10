using System;
using System.Diagnostics;
using System.Threading.Channels;
using System.Threading.Tasks;
using ArcticFox.Codec;
using ArcticFox.Net.Sockets;

namespace ArcticFox.Net.Event
{
    public class NetEventQueue : IBroadcaster, IAsyncDisposable
    {
        private readonly Channel<object> m_queue;
        private CodecChain<byte>? m_preNetTransform;

        // todo: for now we need to use this, as null chain can be specified
        private record SetPreNetTransformMessage(CodecChain<byte>? chain) : IDisposable
        {
            public void Dispose()
            {
                chain?.Dispose();
            }
        }

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
            // we need to take the ref first. otherwise we can race with send and "revive" from 0 refs
            @event.GetRef();
            if (!m_queue.Writer.TryWrite(@event))
            {
                @event.ReleaseRef();
            }
        }

        public void SetPreNetTransform(CodecChainBuilder<byte, byte>? builder)
        {
            var codecChain = builder?.ChainTo(NetEventFactory.s_instance);
            if (!m_queue.Writer.TryWrite(new SetPreNetTransformMessage(codecChain)))
            {
                builder?.Dispose();
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
                    await FlushNetEventOuter(socket, netEvent);
                } else if (message is SetPreNetTransformMessage setTransform)
                {
                    m_preNetTransform?.Dispose();
                    m_preNetTransform = setTransform.chain;
                }
            }
        }

        private async ValueTask FlushNetEventOuter(SocketInterface socket, NetEvent netEvent)
        {
            try
            {
                await FlushNetEvent(socket, netEvent);
            } finally
            {
                netEvent.ReleaseRef();
            }
        }

        private async ValueTask FlushNetEvent(SocketInterface socket, NetEvent netEvent)
        {
            if (m_preNetTransform == null)
            {
                await socket.SendBuffer(netEvent.GetMemory());
                return;
            }
            
            // todo: the codec doesn't need to create net events anymore
            // this could play into async idea...
            object? newEv = null;
            m_preNetTransform.Input(netEvent.GetMemory().Span, ref newEv);
            Debug.Assert(newEv != null);
            var newNetEv = (NetEvent) newEv;

            try
            {
                await socket.SendBuffer(newNetEv.GetMemory());
            } finally
            {
                newNetEv.ReleaseCreationRef();
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
                } else if (message is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }
        
        public ValueTask DisposeAsync()
        {
            DisposePendingEvents();
            m_preNetTransform?.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}