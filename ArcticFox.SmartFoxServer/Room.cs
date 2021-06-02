using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArcticFox.Net.Event;
using ArcticFox.Net.Util;
using Castle.DynamicProxy.Internal;
using Microsoft.Extensions.DependencyInjection;
using Stl.Fusion;

namespace ArcticFox.SmartFoxServer
{
    public class RoomDescription
    {
        public string m_name;
        public User? m_creator;
        public object? m_data;

        public int m_type = RoomTypeIDs.DEFAULT;
        public int m_maxUsers = 50;
        public bool m_isTemporary;
    }
    
    [RegisterComputeService(Lifetime = ServiceLifetime.Transient)]
    public class Room : IBroadcaster
    {
        private static readonly IDFactory s_idFactory = new IDFactory();

        public readonly ulong m_id;
        public readonly Zone m_zone;
        public string m_name => m_description.m_name;
        public int m_type => m_description.m_type;
        public int m_maxUsers => m_description.m_maxUsers;

        private readonly ISystemHandler m_systemHandler;
        
        private readonly RoomDescription m_description;
        private readonly AsyncLockedAccess<Dictionary<ulong, User>> m_users;
        
        public readonly Func<NetEvent, User, ValueTask> m_userExcludeFilter;

        private object? m_data;

        private bool m_canJoin = true;

        public Room(RoomDescription desc, Zone zone, ISystemHandler systemHandler)
        {
            m_id = s_idFactory.Next();
            m_description = desc;
            m_zone = zone;
            m_systemHandler = systemHandler;
            m_data = desc.m_data;
            m_users = new AsyncLockedAccess<Dictionary<ulong, User>>(new Dictionary<ulong, User>());
            m_userExcludeFilter = UserExcludeFilter;
        }
        
        private async ValueTask<bool> CreatorIsGone()
        {
            if (m_description.m_creator == null) return true; // we will still check user count...
            var user = await m_zone.GetUser(m_description.m_creator.m_name);
            return user == null;
        }

        internal async ValueTask AddUser(User user, AsyncLockToken<TypedRoomCollection> userRooms)
        {
            using (var users = await m_users.Get())
            {
                if (!m_canJoin) throw new ObjectDisposedException("can't join room, is shut down");
                if (m_maxUsers != -1 && users.m_value.Count >= m_maxUsers) throw new RoomFullException();
                users.m_value.Add(user.m_id, user);
                userRooms.m_value.AddRoom(this);
            }
            
            await m_systemHandler.UserJoinedRoom(this, user);
            if (m_data is IRoomEventHandler eventHandler)
            {
                await eventHandler.UserJoinedRoom(this, user);
            }
        }

        internal async ValueTask RemoveUser(User user)
        {
            if (await RemoveUserInternal(user) != 0) return;
            
            await CheckTemporaryRoomDeletion();
        }
        
        public async ValueTask CheckTemporaryRoomDeletion()
        {
            if (!m_description.m_isTemporary) return;
            if (!await CreatorIsGone()) return;
            
            using (var users = await m_users.Get())
            {
                if (users.m_value.Count != 0) return;
            }
            
            await m_zone.RemoveRoom(this);
        }

        private async ValueTask<int> RemoveUserInternal(User user)
        {
            int newCount;
            using (var users = await m_users.Get())
            {
                if (!users.m_value.Remove(user.m_id)) return -1;
                newCount = users.m_value.Count;
            }
            
            await m_systemHandler.UserLeftRoom(this, user);
            if (m_data is IRoomEventHandler eventHandler)
            {
                await eventHandler.UserLeftRoom(this, user);
            }
            
            return newCount;
        }

        public async ValueTask Shutdown()
        {
            User[] copiedUserList;
            using (var users = await m_users.Get())
            {
                m_canJoin = false;
                copiedUserList = users.m_value.Values.ToArray();
            }

            foreach (var user in copiedUserList)
            {
                await user.RemoveFromRoom(this);
            }

            if (m_description.m_creator != null)
            {
                await m_description.m_creator.RemoveCreatedRoom(this);
            }
        }
        
        public async ValueTask<T[]> GetAllUserData<T>()
        {
            using (var users = await m_users.Get())
            {
                return users.m_value.Select(x => x.Value.GetUserData<T>()).ToArray();
            }
        }

        private async ValueTask UserExcludeFilter(NetEvent ne, User excludeUser)
        {
            using var users = await m_users.Get();
            foreach (var user in users.m_value)
            {
                if (user.Value.m_id == excludeUser.m_id) continue;
                await user.Value.BroadcastEvent(ne);
            }
        }
        
        public async ValueTask BroadcastEvent(NetEvent ev)
        {
            using var users = await m_users.Get();
            foreach (var user in users.m_value)
            {
                await user.Value.BroadcastEvent(ev);
            }
        }
        
        public void SetData(object data)
        {
            m_data = data;
        }

        public T GetData<T>()
        {
            var data = m_data;
            if (data == null) throw new NullReferenceException(nameof(m_data));
            return (T)data!;
        }
    }
}