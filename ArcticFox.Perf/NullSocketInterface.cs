using System;
using System.Threading.Tasks;
using ArcticFox.Net.Sockets;

namespace ArcticFox.Perf
{
    public class NullSocketInterface : SocketInterface
    {
        public override ValueTask SendBuffer(ReadOnlyMemory<byte> data)
        {
            return ValueTask.CompletedTask;
        }

        public override async ValueTask<int> ReceiveBuffer(Memory<byte> buffer)
        {
            // never receive
            //await Task.Delay(-1);
            
            // receive every 16ms to invoke post-receive event flush
            await Task.Delay(16);
            buffer.Span[0] = 0xFF;
            return 1;
        }

        protected override ValueTask CloseSocket()
        {
            return ValueTask.CompletedTask;
        }
    }
}