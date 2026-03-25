using System;
using ArcticFox.Codec;

namespace ArcticFox.Net.Sockets
{
    // todo: determine if this is needed
    // maybe the socket should be responsible for this?
    public class WebSocketFramingInputCodec : DynamicSizeBufferCodec<byte>
    {
        private readonly WebSocketInterface m_socket;
        
        public WebSocketFramingInputCodec(WebSocketInterface socket, int maxMessageSize) : base(maxMessageSize)
        {
            m_socket = socket;
        }
        
        public override void Input(ReadOnlySpan<byte> input, ref object? state)
        {
            if (m_socket.m_lastRecvWasEndOfMessage)
            {
                ExtendFinalMemory(ref input, input, ref state);
            } else
            {
                ExtendMemory(ref input);
            }
        }

        protected override void Reset()
        {
        }
    }
}