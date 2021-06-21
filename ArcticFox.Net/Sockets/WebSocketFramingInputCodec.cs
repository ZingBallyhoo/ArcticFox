using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using ArcticFox.Codec;
using CommunityToolkit.HighPerformance.Buffers;

namespace ArcticFox.Net.Sockets
{
    public class WebSocketFramingInputCodec : SpanCodec<byte, byte>, IDisposable
    {
        private readonly int m_maxMessageSize;
        private IMemoryOwner<byte>? m_memory;
        
        public WebSocketFramingInputCodec(int maxMessageSize)
        {
            m_maxMessageSize = maxMessageSize;
        }
        
        public override void Input(ReadOnlySpan<byte> input, ref object? state)
        {
            Debug.Assert(state != null);
            var webSocket = (WebSocketInterface)state;

            if (webSocket.m_lastRecvWasEndOfMessage && m_memory == null)
            {
                // todo: do we want to enforce m_maxMessageSize here? not much point as its fixed size
                
                // we have the whole message
                CodecOutput(input, ref state);
                return;
            }
            
            // todo: proper growth strategy

            var sizeNeeded = input.Length;
            var thisWriteOffset = 0;
            var existingMemoryOwner = m_memory;
            if (existingMemoryOwner != null)
            {
                // consider how much we already have
                var existingMemorySize = existingMemoryOwner.Memory.Length;
                thisWriteOffset = existingMemorySize;
                sizeNeeded += existingMemorySize;
            }

            if (sizeNeeded > m_maxMessageSize)
            {
                Abort();
                throw new InvalidDataException($"websocket message too big. {sizeNeeded} > {m_maxMessageSize}");
            }

            var newMemoryOwner = MemoryOwner<byte>.Allocate(sizeNeeded);
            m_memory = newMemoryOwner;
            
            if (existingMemoryOwner != null)
            {
                // copy existing data into new buffer
                existingMemoryOwner.Memory.CopyTo(newMemoryOwner.Memory);
                existingMemoryOwner.Dispose();
                existingMemoryOwner = null; // die
            }
            
            // copy new data
            input.CopyTo(newMemoryOwner.Memory.Slice(thisWriteOffset, sizeNeeded-thisWriteOffset).Span);

            if (!webSocket.m_lastRecvWasEndOfMessage) return;
            
            CodecOutput(newMemoryOwner.Span, ref state);
            RemoveMemory();
        }

        private void RemoveMemory()
        {
            m_memory?.Dispose();
            m_memory = null;
        }

        public void Dispose()
        {
            RemoveMemory();
        }
    }
}