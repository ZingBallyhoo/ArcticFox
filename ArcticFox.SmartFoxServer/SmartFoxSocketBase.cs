using System.Threading.Tasks;
using ArcticFox.Net;
using ArcticFox.Net.Sockets;

namespace ArcticFox.SmartFoxServer
{
    public class SmartFoxSocketBase : HighLevelSocket
    {
        protected readonly SmartFoxManager m_manager;
        
        public SmartFoxSocketBase(SocketInterface socket, SmartFoxManager manager) : base(socket)
        {
            m_manager = manager;
        }

        public ValueTask<User> CreateUser(string zoneName, string name)
        {
            return m_manager.CreateUser(name, this, zoneName);
        }

        public override async ValueTask DisposeAsync()
        {
            await m_manager.LogoutSocket(this);
            await base.DisposeAsync();
        }
    }
}