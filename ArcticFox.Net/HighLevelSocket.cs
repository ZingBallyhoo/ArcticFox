using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ArcticFox.Codec;
using ArcticFox.Net.Batching;
using ArcticFox.Net.Event;
using ArcticFox.Net.Sockets;
using ArcticFox.Net.Util;

namespace ArcticFox.Net
{
    public class HighLevelSocket : IBroadcaster, IDisposable
    {
        public readonly SocketInterface m_socket;
        public readonly TaskQueue m_taskQueue;
        private readonly NetEventQueue m_netEventQueue;

        protected CodecChain? m_netInputCodec;

        private bool m_hasPreNetTransform;
        protected AsyncLockedAccess<CodecChain<byte, byte>?> m_preNetTransform;

        public HighLevelSocket(SocketInterface socket)
        {
            m_socket = socket;
            m_taskQueue = new TaskQueue();
            m_netEventQueue = new NetEventQueue();
            m_preNetTransform = new AsyncLockedAccess<CodecChain<byte, byte>?>(null);
        }

        protected void SetPreNetTransform(CodecChain<byte, byte>? chain)
        {
            // todo: i guess its fine to block here
            using var token = m_preNetTransform.GetSync();
            m_hasPreNetTransform = chain != null;
            var current = token.m_value;
            token.m_value = chain;
            if (current != null)
            {
                current.Dispose();
            }
        }
        
        public virtual void NetworkInput(ReadOnlySpan<byte> data)
        {
            var inputCodec = m_netInputCodec;
            if (inputCodec == null) throw new NullReferenceException("No input codec");
            
            inputCodec.Head<byte>().Input2(data, m_socket);
        }

        public void Close()
        {
            m_socket.Close();
        }

        public bool IsClosed()
        {
            return m_socket.IsClosed();
        }
        
        public virtual ValueTask<int> HandlePendingSendEvents(ISendContext ctx)
        {
            return m_netEventQueue.FlushEventsToSocket(m_socket, ctx);
        }
        
        public virtual void HandleException(Exception e)
        {
            // todo: ... exception handling
            Console.Out.WriteLine(e.ToString());
        }
        
        public ValueTask BroadcastEvent(NetEvent ev)
        {
            if (m_hasPreNetTransform)
            {
                return BroadcastWithTransform(ev);
            } else
            {
                return m_netEventQueue.BroadcastEvent(ev);
            }
        }

        private async ValueTask BroadcastWithTransform(NetEvent ev)
        {
            using var transformToken = await m_preNetTransform.Get();
            var transform = transformToken.m_value;
            if (transform == null)
            {
                // shouldn't have gotten here, but lets handle it anyway
                await m_netEventQueue.BroadcastEvent(ev);
                return;
            }
            
            object? newEv = null;
            transform.Input(ev.GetMemory().Span, ref newEv);
            Debug.Assert(newEv != null);
            var newNetEv = (NetEvent) newEv;
            await m_netEventQueue.BroadcastEventOwningCreation(newNetEv);
        }

        public void Dispose()
        {
            Close();
        }

        public virtual async ValueTask CleanupAsync()
        {
            await m_netEventQueue.DisposeAsync();
            m_netInputCodec?.Dispose();
            m_netInputCodec = null;
            SetPreNetTransform(null);
        }
    }
}