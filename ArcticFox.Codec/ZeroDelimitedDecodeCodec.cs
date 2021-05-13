using System;
using CommunityToolkit.HighPerformance.Buffers;

namespace ArcticFox.Codec
{
    public class ZeroDelimitedDecodeCodec : SpanCodec<byte, byte>, IDisposable
    {
        private MemoryOwner<byte> m_recvBuffer = MemoryOwner<byte>.Empty;
        private int m_recvBufferPos;

        public bool m_canGrow = true;

        public override void Input(ReadOnlySpan<byte> input, object? state)
        {
            var packetOffset = 0;
            while (packetOffset < input.Length)
            {
                var packetToEndSpan = input.Slice(packetOffset);
                var idxOf0 = packetToEndSpan.IndexOf((byte)0);

                int packetSize;
                if (idxOf0 == -1) packetSize = packetToEndSpan.Length;
                else packetSize = idxOf0;

                var packetSpan = packetToEndSpan.Slice(0, packetSize);
                packetOffset += packetSize;
                if (idxOf0 != -1)
                {
                    packetOffset += 1; // ignore 0
                }

                if (idxOf0 != -1 && m_recvBufferPos == 0)
                {
                    // no need to copy, we have received all in 1 blob
                    CodecOutput(packetSpan, state);
                    continue;
                }
                
                if (!GrowBufferBy(packetSize)) break;
                packetSpan.CopyTo(m_recvBuffer.Span.Slice(m_recvBufferPos));
                
                if (idxOf0 != -1)
                {
                    CodecOutput(m_recvBuffer.Span.Slice(0, m_recvBufferPos), state);
                    m_recvBufferPos = 0;
                } else
                {
                    m_recvBufferPos += packetSize;
                }
            }
        }
        
        private bool GrowBufferBy(int size)
        {
            var currentBuffer = m_recvBuffer;
            var targetLen = size + m_recvBufferPos;
            if (currentBuffer.Length >= targetLen) return true;
            
            if (!m_canGrow)
            {
                //Log.Error("Disconnecting {Endpoint} because the read buffer can't grow (to {Size} bytes)", GetIdentifier(), targetLen);
                Abort();
                return false;
            }

            var newBuffer = MemoryOwner<byte>.Allocate(targetLen);
            currentBuffer.Memory.CopyTo(newBuffer.Memory);
            m_recvBuffer = newBuffer;
            currentBuffer.Dispose();

            return true;
        }

        private void DisposeCurrentBuffer()
        {
            var recvBuffer = m_recvBuffer;
            m_recvBuffer = MemoryOwner<byte>.Empty;
            recvBuffer.Dispose();
        }
        
        public virtual void Dispose()
        {
            DisposeCurrentBuffer();
            GC.SuppressFinalize(this);
        }
        
        ~ZeroDelimitedDecodeCodec()
        {
            DisposeCurrentBuffer();
        }
    }
}