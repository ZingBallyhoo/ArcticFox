using System;
using System.Collections.Generic;
using System.Text;
using ArcticFox.Codec;
using ArcticFox.Net;
using ArcticFox.Net.Sockets;

namespace ArcticFox.Tests.Impls
{
    public class TestSocket : HighLevelSocket, ISpanConsumer<char>
    {
        public List<string> m_received;
        
        public TestSocket(SocketInterface socket) : base(socket)
        {
            m_netInputCodec = new CodecChain().AddCodec(new ZeroDelimitedDecodeCodec()).AddCodec(new TextDecodeCodec(Encoding.ASCII)).AddCodec(this);
            m_received = new List<string>();
        }

        public void Input(ReadOnlyMemory<char> input, ref object? state)
        {
            m_received.Add(input.ToString());
        }

        public void Abort()
        {
            Close();
        }
    }
}