using System;
using System.Threading.Tasks;
using ArcticFox.Net;
using ArcticFox.Net.Sockets;

namespace ArcticFox.SmartFoxServer
{
    public class SmartFoxSocketBase : HighLevelSocket
    {
        protected readonly SmartFoxManager m_manager;
        private User? m_user;
        
        public SmartFoxSocketBase(SocketInterface socket, SmartFoxManager manager) : base(socket)
        {
            m_manager = manager;
        }

        public async ValueTask<User> CreateUser(string zoneName, string name)
        {
            var user = await m_manager.CreateUser(name, this, zoneName);
            m_user = user;
            return user;
        }

        public User GetUser()
        {
            var user = m_user;
            if (user == null) throw new NullReferenceException("never logged in");
            if (user.IsShutDown()) throw new ObjectDisposedException("user logged out");
            return user;
        }

        public override async ValueTask CleanupAsync()
        {
            await m_manager.LogoutSocket(this);
            await base.CleanupAsync();
        }
    }
}