using ArcticFox.Net;
using ArcticFox.Net.Sockets;

namespace ArcticFox.Perf
{
    public class NullSocketHost : SocketHost
    {
        public override HighLevelSocket CreateHighLevelSocket(SocketInterface socket)
        {
            return new NullSocket(socket);
        }
    }
}