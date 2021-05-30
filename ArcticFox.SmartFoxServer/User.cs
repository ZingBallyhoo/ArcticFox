using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ArcticFox.Net;
using ArcticFox.Net.Event;
using ArcticFox.Net.Util;
using Castle.DynamicProxy.Internal;
using Microsoft.Extensions.DependencyInjection;
using Stl.Fusion;

namespace ArcticFox.SmartFoxServer
{
    public class UserDescription
    {
        public string m_name;
        public HighLevelSocket? m_socket;
    }
    
    [RegisterComputeService(Lifetime = ServiceLifetime.Transient)]
    public class User : IBroadcaster
    {
        private static readonly IDFactory s_idFactory = new IDFactory();

        public readonly ulong m_id;
        public readonly Zone m_zone;
        public string m_name => m_description.m_name;
        public HighLevelSocket? m_socket => m_description.m_socket;

        private readonly UserDescription m_description;
        private readonly AsyncLockedAccess<HashSet<Room>> m_createdRooms;
        private readonly AsyncLockedAccess<RoomCollection> m_rooms;

        private object? m_userData;

        private bool m_canJoinRooms = true;

        public User(UserDescription description, Zone zone, RoomCollection rooms)
        {
            m_id = s_idFactory.Next();
            m_description = description;
            m_zone = zone;
            m_createdRooms = new AsyncLockedAccess<HashSet<Room>>(new HashSet<Room>());
            m_rooms = new AsyncLockedAccess<RoomCollection>(rooms);
        }
        
        public ValueTask BroadcastEvent(NetEvent ev)
        {
            if (m_socket == null) return ValueTask.CompletedTask;
            return m_socket.BroadcastEvent(ev);
        }
        
        public async ValueTask AddCreatedRoom(Room room)
        {
            using var _ = await m_rooms.Get();
            using var createdRooms = await m_createdRooms.Get();
            if (!m_canJoinRooms) throw new Exception("can't create room as m_canJoinRooms is false");
            createdRooms.m_value.Add(room);
        }
        
        public async ValueTask RemoveCreatedRoom(Room room)
        {
            using var _ = await m_rooms.Get();
            using var createdRooms = await m_createdRooms.Get();
            createdRooms.m_value.Remove(room);
        }

        public async ValueTask AddRoomToList(Room room)
        {
            using var rooms = await m_rooms.Get();
            if (!m_canJoinRooms) throw new Exception("can't add room as m_canJoinRooms is false");
            rooms.m_value.AddRoom(room);
        }
        
        public async ValueTask RemoveRoomFromList(Room room)
        {
            using var rooms = await m_rooms.Get();
            rooms.m_value.RemoveRoom(room);
        }

        public async ValueTask<Room> GetPrimaryRoom()
        {
            using var rooms = await m_rooms.Get();
            if (rooms.m_value.m_roomsByName.Count != 1) throw new InvalidDataException($"{nameof(GetPrimaryRoom)}: in {rooms.m_value.m_roomsByName.Count} rooms");
            return rooms.m_value.m_roomsByName.Values.First();
        }
        
        public async ValueTask<Room?> GetPrimaryRoomOrNull()
        {
            using var rooms = await m_rooms.Get();
            if (rooms.m_value.m_roomsByName.Count != 1) return null;
            return rooms.m_value.m_roomsByName.Values.First();
        }

        public void SetUserData(object? userData)
        {
            m_userData = userData;
        }

        public T GetUserData<T>()
        {
            var userData = m_userData;
            if (!typeof(T).IsNullableType() && userData == null) // todo: checking IsNullableType every time...
            {
                throw new NullReferenceException(nameof(m_userData));
            }
            return (T)userData!;
        }

        public async ValueTask Shutdown()
        {
            Room[] copiedRoomList;
            Room[] copiedCreatedRoomList;
            using (var rooms = await m_rooms.Get())
            using (var createdRooms = await m_createdRooms.Get())
            {
                m_canJoinRooms = false;
                copiedRoomList = rooms.m_value.m_roomsByID.Values.ToArray();
                copiedCreatedRoomList = createdRooms.m_value.ToArray();
            }
            
            foreach (var room in copiedRoomList)
            {
                await room.RemoveUser(this);
            }
            foreach (var room in copiedCreatedRoomList)
            {
                await room.CheckTemporaryRoomDeletion();
            }
        }

        public bool IsShutDown() => !m_canJoinRooms;
    }
}