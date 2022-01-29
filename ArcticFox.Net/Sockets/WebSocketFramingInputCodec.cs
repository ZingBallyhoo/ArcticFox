using System;
using System.Diagnostics;
using ArcticFox.Codec;

namespace ArcticFox.Net.Sockets
{
    public class WebSocketFramingInputCodec : DynamicSizeBufferCodec<byte>
    {
        public WebSocketFramingInputCodec(int maxMessageSize)
        {
            m_maxMemorySize = maxMessageSize;
        }
        
        public override void Input(ReadOnlySpan<byte> input, ref object? state)
        {
            Debug.Assert(state != null);
            var webSocket = (WebSocketInterface)state;

            if (webSocket.m_lastRecvWasEndOfMessage)
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