using System;
using System.Threading.Tasks;
using ArcticFox.Codec;
using ArcticFox.Net.Event;
using ArcticFox.Net.Sockets;
using ArcticFox.Net.Util;

namespace ArcticFox.Net
{
    public class HighLevelSocket : IBroadcaster, IDisposable
    {
        public readonly SocketInterface m_socket;
        public readonly TaskQueue m_taskQueue;
        internal readonly NetEventQueue m_netEventQueue;

        protected CodecChain<byte>? m_netInputCodec;

        public HighLevelSocket(SocketInterface socket)
        {
            m_socket = socket;
            m_taskQueue = new TaskQueue();
            m_netEventQueue = new NetEventQueue();
        }

        protected void SetPreNetTransform(CodecChainBuilder<byte, byte>? chain)
        {
            m_netEventQueue.SetPreNetTransform(chain);
        }
        
        public virtual void NetworkInput(ReadOnlySpan<byte> data)
        {
            var inputCodec = m_netInputCodec;
            if (inputCodec == null) throw new NullReferenceException("No input codec");
            
            inputCodec.Input(data, m_socket);
        }

        public void Close()
        {
            m_socket.Close();
        }

        public bool IsClosed()
        {
            return m_socket.IsClosed();
        }
        
        public virtual void HandleException(Exception e)
        {
            // todo: ... exception handling
            Console.Out.WriteLine(e.ToString());
        }
        
        public virtual ValueTask BroadcastEvent(NetEvent ev)
        {
            return m_netEventQueue.BroadcastEvent(ev);
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