using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ArcticFox.Net.Sockets;

namespace ArcticFox.Net.Batching
{
    public class BatchedSendContext : ISendContext
    {
        private int m_maxSize => m_sharedBuffer.Length;
        private readonly Memory<byte> m_sharedBuffer;

        private int m_sharedBufferOffset;
        
        public BatchedSendContext(Memory<byte> sharedBuffer)
        {
            m_sharedBuffer = sharedBuffer;
            m_sharedBufferOffset = 0;
        }

        public async Task AddMessage(SocketInterface socket, ReadOnlyMemory<byte> arr, int remainingAddCount)
        {
            var length = arr.Length;
            
            var remainingSize = length;
            bool sendingExistingBuffer;
            while (remainingSize > 0)
            {
                if (m_sharedBufferOffset == 0)
                {
                    if (remainingSize > m_maxSize || remainingAddCount == 0)
                    {
                        sendingExistingBuffer = true;
                    } else
                    {
                        Debug.Assert(m_sharedBufferOffset == 0); // hmm... sanity
                            
                        // need to use shared buffer to write multiple packets together
                        sendingExistingBuffer = false;
                    }
                } else
                {
                    sendingExistingBuffer = false;
                }

                if (sendingExistingBuffer)
                {
                    var writeCount = Math.Min(remainingSize, m_maxSize);
                    await socket.SendBuffer(arr, length - remainingSize, writeCount);
                    remainingSize -= writeCount;
                } else
                {
                    var writeCount = Math.Min(remainingSize, m_maxSize - m_sharedBufferOffset);
                    arr.Slice(length - remainingSize, writeCount).CopyTo(m_sharedBuffer.Slice(m_sharedBufferOffset, writeCount));
                    
                    m_sharedBufferOffset += writeCount;
                    remainingSize -= writeCount;

                    if (m_sharedBufferOffset == m_maxSize)
                    {
                        await socket.SendBuffer(m_sharedBuffer, 0, m_sharedBufferOffset);
                        m_sharedBufferOffset = 0;
                    }
                }
            }
        }

        public async Task Flush(SocketInterface socket)
        {
            if (m_sharedBufferOffset == 0) return;
            await socket.SendBuffer(m_sharedBuffer, 0, m_sharedBufferOffset);
            m_sharedBufferOffset = 0;
        }
    }
}