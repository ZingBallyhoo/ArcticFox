using System;
using ArcticFox.Codec;
using ArcticFox.Net;
using ArcticFox.Net.Sockets;

namespace ArcticFox.Perf
{
    public class NullSocket : HighLevelSocket, ISpanConsumer<byte>
    {
        public NullSocket(SocketInterface socket) : base(socket)
        {
            m_netInputCodec = new CodecChain<byte>(this);
        }

        public void Input(ReadOnlySpan<byte> input, ref object? state)
        {
        }
    }
}