using ArcticFox.Net;
using ArcticFox.Net.Sockets;

namespace ArcticFox.Tests.Impls
{
    public class TestSocketHost : SocketHost
    {
        public override HighLevelSocket CreateHighLevelSocket(SocketInterface socket)
        {
            return new TestSocket(socket);
        }
    }
}