using System;
using System.Threading.Tasks;
using ArcticFox.Net.Sockets;

namespace ArcticFox.Net.Batching
{
    public class NormalSendContext : ISendContext
    {
        public Task AddMessage(SocketInterface socket, ReadOnlyMemory<byte> arr, int remainingAddCount)
        {
            return socket.SendBuffer(arr);
        }

        public Task Flush(SocketInterface socket)
        {
            return Task.CompletedTask;
        }
    }
}