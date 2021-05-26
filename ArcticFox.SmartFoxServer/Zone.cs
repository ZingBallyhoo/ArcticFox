using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArcticFox.Net;
using ArcticFox.Net.Event;
using ArcticFox.Net.Util;
using Microsoft.Extensions.DependencyInjection;
using Stl.Async;
using Stl.DependencyInjection;
using Stl.Fusion;

namespace ArcticFox.SmartFoxServer
{
    [RegisterService(Lifetime = ServiceLifetime.Scoped)]
    public class ZoneDefinition
    {
        public string m_name;
        public IServiceScope m_scope;
    }

    [RegisterComputeService(Lifetime = ServiceLifetime.Transient)]
    public class RoomCollection
    {
        public readonly Dictionary<ulong, Room> m_roomsByID = new Dictionary<ulong, Room>();
        public readonly Dictionary<string, Room> m_roomsByName = new Dictionary<string, Room>();

        public void AddRoom(Room room)
        {
            m_roomsByID.Add(room.m_id, room);
            m_roomsByName.Add(room.m_name, room);
            
            using (Computed.Invalidate())
            {
                GetByName(room.m_name).Ignore();
            }
        }

        public void RemoveRoom(Room room)
        {
            m_roomsByID.Remove(room.m_id);
            m_roomsByName.Remove(room.m_name);
            
            using (Computed.Invalidate())
            {
                GetByName(room.m_name).Ignore();
            }
        }

        [ComputeMethod]
        public virtual Task<Room?> GetByName(string name)
        {
            m_roomsByName.TryGetValue(name, out var room);
            return Task.FromResult(room);
        }
    }

    [RegisterComputeService(Lifetime = ServiceLifetime.Transient)]
    public class ZoneUsers
    {
        public readonly Dictionary<ulong, User> m_usersByID = new Dictionary<ulong, User>();
        public readonly Dictionary<string, User> m_usersByName = new Dictionary<string, User>();
        public readonly Dictionary<HighLevelSocket, User> m_usersBySocket = new Dictionary<HighLevelSocket, User>();

        public void AddUser(User user)
        {
            m_usersByID.Add(user.m_id, user);
            m_usersByName.Add(user.m_name, user);
            if (user.m_socket != null) m_usersBySocket.Add(user.m_socket, user);
            
            using (Computed.Invalidate())
            {
                GetByName(user.m_name).Ignore();
            }
        }

        public void RemoveUser(User user)
        {
            m_usersByID.Remove(user.m_id);
            m_usersByName.Remove(user.m_name);
            if (user.m_socket != null) m_usersBySocket.Remove(user.m_socket);
            
            using (Computed.Invalidate())
            {
                GetByName(user.m_name).Ignore();
            }
        }
        
        [ComputeMethod]
        public virtual Task<User?> GetByName(string name)
        {
            m_usersByName.TryGetValue(name, out var user);
            return Task.FromResult(user);
        }
    }
    
    [RegisterComputeService(Lifetime = ServiceLifetime.Scoped)]
    public class Zone : IDisposable, IBroadcaster
    {
        private readonly AsyncLockedAccess<ZoneUsers> m_users;
        private readonly AsyncLockedAccess<RoomCollection> m_rooms;

        private readonly IServiceProvider m_provider;
        private readonly ISystemHandler m_systemHandler;
        private readonly ZoneDefinition m_definition;

        public string m_name => m_definition.m_name;

        public Zone(ZoneDefinition definition, IServiceProvider provider, ZoneUsers zoneUsers, RoomCollection roomCollection, ISystemHandler systemHandler)
        {
            m_definition = definition;
            m_provider = provider;
            m_systemHandler = systemHandler;
            m_users = new AsyncLockedAccess<ZoneUsers>(zoneUsers);
            m_rooms = new AsyncLockedAccess<RoomCollection>(roomCollection);
        }

        public async ValueTask<User> CreateUser(string name, HighLevelSocket? socket)
        {
            var user = m_provider.Activate<User>(new UserDescription
            {
                m_name = name,
                m_socket = socket
            });
            using var users = await m_users.Get();
            users.m_value.AddUser(user);
            return user;
        }
        
        public async ValueTask RemoveUser(User user)
        {
            user.m_socket?.Close();
            using (var users = await m_users.Get())
            {
                users.m_value.RemoveUser(user);
            }
            await user.Shutdown();
        }
        
        public async ValueTask<Room> CreateRoom(RoomDescription desc)
        {
            var room = m_provider.Activate<Room>(desc);
            using (var rooms = await m_rooms.Get())
            {
                if (desc.m_creator != null) await desc.m_creator.AddCreatedRoom(room);
                rooms.m_value.AddRoom(room);
            }
            await m_systemHandler.RoomCreated(room);
            return room;
        }

        public ValueTask<Room> CreateRoom(string name)
        {
            return CreateRoom(new RoomDescription
            {
                m_name = name
            });
        }

        public async ValueTask RemoveRoom(Room room)
        {
            using (var rooms = await m_rooms.Get())
            {
                rooms.m_value.RemoveRoom(room);
                await room.Shutdown();
            }
            await m_systemHandler.RoomRemoved(room);
        }

        public void Dispose()
        {
            m_definition.m_scope.Dispose();
        }

        public async ValueTask BroadcastEvent(NetEvent ev)
        {
            using var rooms = await m_rooms.Get();
            foreach (var room in rooms.m_value.m_roomsByID)
            {
                await room.Value.BroadcastEvent(ev);
            }
        }
        
        [ComputeMethod]
        public virtual async ValueTask<User?> GetUser(string name)
        {
            using var rooms = await m_users.Get();
            return await rooms.m_value.GetByName(name);
        }
        
        [ComputeMethod]
        public virtual async ValueTask<Room?> GetRoom(string name)
        {
            using var rooms = await m_rooms.Get();
            return await rooms.m_value.GetByName(name);
        }
    }
}