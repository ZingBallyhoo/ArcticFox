using System;
using System.Threading.Tasks;
using ArcticFox.Net.Sockets;

namespace ArcticFox.Net.Batching
{
    public class NormalSendContext : ISendContext
    {
        public ValueTask AddMessage(SocketInterface socket, ReadOnlyMemory<byte> arr, int remainingAddCount)
        {
            return socket.SendBuffer(arr);
        }

        public ValueTask Flush(SocketInterface socket)
        {
            return ValueTask.CompletedTask;
        }
    }
}