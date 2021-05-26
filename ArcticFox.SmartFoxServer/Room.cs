using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArcticFox.Net.Event;
using ArcticFox.Net.Util;
using Microsoft.Extensions.DependencyInjection;
using Stl.Fusion;

namespace ArcticFox.SmartFoxServer
{
    public class RoomDescription
    {
        public string m_name;
        public User? m_creator;
        public int m_maxUsers = 10;
        
        public bool m_isTemporary;
    }
    
    [RegisterComputeService(Lifetime = ServiceLifetime.Transient)]
    public class Room : IBroadcaster
    {
        private static readonly IDFactory s_idFactory = new IDFactory();

        public readonly ulong m_id;
        public readonly Zone m_zone;
        public string m_name => m_description.m_name;

        private readonly ISystemHandler m_systemHandler;
        
        private readonly RoomDescription m_description;
        private readonly AsyncLockedAccess<Dictionary<ulong, User>> m_users;

        
        private bool m_canJoin = true;

        public Room(RoomDescription desc, Zone zone, ISystemHandler systemHandler)
        {
            m_id = s_idFactory.Next();
            m_description = desc;
            m_zone = zone;
            m_systemHandler = systemHandler;
            m_users = new AsyncLockedAccess<Dictionary<ulong, User>>(new Dictionary<ulong, User>());
        }
        
        private async ValueTask<bool> CreatorIsGone()
        {
            if (m_description.m_creator == null) return true; // we will still check user count...
            var user = await m_zone.GetUser(m_description.m_creator.m_name);
            return user == null;
        }

        public async ValueTask AddUser(User user)
        {
            using (var users = await m_users.Get())
            {
                await user.AddRoomToList(this);
                if (!m_canJoin) throw new Exception("can't join room, is shut down");
                users.m_value.Add(user.m_id, user);
            }
            await m_systemHandler.UserJoinedRoom(this, user);
        }

        public async ValueTask RemoveUser(User user)
        {
            if (await RemoveUserInternal(user) == 0)
            {
                await CheckTemporaryRoomDeletion();
            }
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
            await user.RemoveRoomFromList(this);
            await m_systemHandler.UserLeftRoom(this, user);
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
                await RemoveUserInternal(user);
            }

            if (m_description.m_creator != null)
            {
                await m_description.m_creator.RemoveCreatedRoom(this);
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
    }
}