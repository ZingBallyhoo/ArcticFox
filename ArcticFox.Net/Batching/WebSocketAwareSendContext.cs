using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ArcticFox.Net.Sockets;

namespace ArcticFox.Net.Batching
{
    public class WebSocketAwareSendContext  : ISendContext
    {
        private readonly int m_maxMessageSize;
        
        public WebSocketAwareSendContext(int maxMessageSize)
        {
            m_maxMessageSize = maxMessageSize;
        }
        
        public async ValueTask AddMessage(SocketInterface socket, ReadOnlyMemory<byte> arr, int remainingAddCount)
        {
            var webSocket = (WebSocketInterface) socket;
            
            var remainingLength = arr.Length;

            while (remainingLength > 0)
            {
                var sizeToSend = Math.Min(remainingLength, m_maxMessageSize);
                var nextRemainingLength = remainingLength - sizeToSend;
                Debug.Assert(nextRemainingLength < remainingLength);
                Debug.Assert(nextRemainingLength >= 0);

                var isEndOfPacket = nextRemainingLength <= 0;

                var sliceToSend = arr.Slice(arr.Length - remainingLength, sizeToSend);
                await webSocket.SendBuffer(sliceToSend, isEndOfPacket);

                remainingLength = nextRemainingLength;
            }
        }
        
        public ValueTask Flush(SocketInterface socket)
        {
            return ValueTask.CompletedTask;
        }
    }
}