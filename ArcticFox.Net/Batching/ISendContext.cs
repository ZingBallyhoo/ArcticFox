using System;
using System.Threading.Tasks;
using ArcticFox.Net.Sockets;

namespace ArcticFox.Net.Batching
{
    public interface ISendContext
    {
        ValueTask AddMessage(SocketInterface socket, ReadOnlyMemory<byte> arr, int remainingAddCount);
        ValueTask Flush(SocketInterface socket);
    }
}