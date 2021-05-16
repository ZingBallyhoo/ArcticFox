using System;
using System.Threading.Tasks;
using ArcticFox.Net.Sockets;

namespace ArcticFox.Net.Batching
{
    public interface ISendContext
    {
        Task AddMessage(SocketInterface socket, ReadOnlyMemory<byte> arr, int remainingAddCount);
        Task Flush(SocketInterface socket);
    }
}