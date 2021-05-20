using System;
using System.Threading.Tasks;
using ArcticFox.Codec;
using ArcticFox.Net.Batching;
using ArcticFox.Net.Event;
using ArcticFox.Net.Sockets;

namespace ArcticFox.Net
{
    public class HighLevelSocket : IBroadcaster, IAsyncDisposable
    {
        public readonly SocketInterface m_socket;
        private readonly NetEventQueue m_netEventQueue;

        protected CodecChain? m_netInputCodec;
        protected CodecChain<byte, byte>? m_preNetTransform;

        public HighLevelSocket(SocketInterface socket)
        {
            m_socket = socket;
            m_netEventQueue = new NetEventQueue();
        }

        public virtual void NetworkInput(ReadOnlyMemory<byte> data)
        {
            var inputCodec = m_netInputCodec;
            if (inputCodec == null) throw new NullReferenceException("No input codec");
            inputCodec.Head<byte>().Input2(data);
        }

        public void Close()
        {
            m_socket.Close();
        }

        public bool IsClosed()
        {
            return m_socket.IsClosed();
        }
        
        public Task<int> HandlePendingSendEvents(ISendContext ctx)
        {
            return m_netEventQueue.FlushEventsToSocket(m_socket, ctx);
        }
        
        public ValueTask BroadcastEvent(NetEvent ev)
        {
            //if (m_preNetTransform != null)
            //{
            //    lock (m_preNetTransform) // sanity: prevent stateful crypto out of order or corruption?
            //    {
            //        m_preNetTransform.Input2(ev.GetMemory().Span, m_netEventQueue);
            //    }
            //} else
            //{
                return m_netEventQueue.BroadcastEvent(ev);
            //}
        }

        public async ValueTask DisposeAsync()
        {
            await m_netEventQueue.DisposeAsync();
            m_socket.Dispose();
        }
    }
}