using System;
using System.Collections.Generic;
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
    public record UserDescription(string m_name, HighLevelSocket? m_socket);

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
        private readonly AsyncLockedAccess<TypedRoomCollection> m_rooms;

        private object? m_userData;

        private bool m_canJoinRooms = true;

        public User(UserDescription description, Zone zone, TypedRoomCollection rooms)
        {
            m_id = s_idFactory.Next();
            m_description = description;
            m_zone = zone;
            m_createdRooms = new AsyncLockedAccess<HashSet<Room>>(new HashSet<Room>());
            m_rooms = new AsyncLockedAccess<TypedRoomCollection>(rooms);
        }
        
        public ValueTask BroadcastEvent(NetEvent ev)
        {
            if (m_socket == null) return ValueTask.CompletedTask;
            return m_socket.BroadcastEvent(ev);
        }
        
        internal async ValueTask AddCreatedRoom(Room room)
        {
            using var _ = await m_rooms.Get();
            using var createdRooms = await m_createdRooms.Get();
            if (!m_canJoinRooms) throw new ObjectDisposedException("can't create room as m_canJoinRooms is false");
            createdRooms.m_value.Add(room);
        }
        
        public ValueTask MoveTo(Room room)
        {
            return MoveTo(room, room.m_type);
        }

        public ValueTask RemoveFromRoom(int type)
        {
            return MoveTo(null, type);
        }
        
        internal async ValueTask RemoveFromRoom(Room room)
        {
            using var rooms = await m_rooms.Get();
            if (!rooms.m_value.m_roomsByType.TryGetValue(room.m_type, out var currentRoom)) return;
            if (currentRoom.m_id != room.m_id) return;
            await RemoveFromRoomInternal(room, rooms);
        }

        private async ValueTask RemoveFromRoomInternal(Room room, AsyncLockToken<TypedRoomCollection> rooms)
        {
            await room.RemoveUser(this);
            rooms.m_value.RemoveRoom(room);
        }
        
        private async ValueTask MoveTo(Room? room, int type)
        {
            using var rooms = await m_rooms.Get();
            if (rooms.m_value.m_roomsByType.TryGetValue(type, out var currentRoom))
            {
                await RemoveFromRoomInternal(currentRoom, rooms);
            }
            
            if (room != null)
            {
                if (!m_canJoinRooms) throw new ObjectDisposedException("can't add room as m_canJoinRooms is false");
                await room.AddUser(this, rooms);
            }
        }
        
        internal async ValueTask RemoveCreatedRoom(Room room)
        {
            // todo: not locking m_rooms here, could be locked in MoveTo way outer?
            using var createdRooms = await m_createdRooms.Get();
            createdRooms.m_value.Remove(room);
        }

        public async ValueTask<Room> GetRoom(int type = RoomTypeIDs.DEFAULT)
        {
            var room = await GetRoomOrNull(type);
            if (room != null) return room;
            throw new KeyNotFoundException($"Not in room of type {RoomTypeIDs.GetName(type)} ({type})");
        }
        
        public async ValueTask<Room?> GetRoomOrNull(int type = RoomTypeIDs.DEFAULT)
        {
            using var rooms = await m_rooms.Get();
            rooms.m_value.m_roomsByType.TryGetValue(type, out var room);
            return room;
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
                copiedRoomList = rooms.m_value.m_roomsByType.Values.ToArray();
                copiedCreatedRoomList = createdRooms.m_value.ToArray();
            }
            
            foreach (var room in copiedRoomList)
            {
                await RemoveFromRoom(room);
            }
            foreach (var room in copiedCreatedRoomList)
            {
                await room.CheckTemporaryRoomDeletion();
            }
        }

        public bool IsShutDown() => !m_canJoinRooms;
    }
}