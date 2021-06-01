using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Stl.Fusion;

namespace ArcticFox.SmartFoxServer
{
    [RegisterComputeService(Lifetime = ServiceLifetime.Transient)]
    public class TypedRoomCollection
    {
        public readonly Dictionary<int, Room> m_roomsByType = new Dictionary<int, Room>();

        public void AddRoom(Room room)
        {
            if (m_roomsByType.ContainsKey(room.m_type))
            {
                throw new ArgumentException();
            }
            
            m_roomsByType.Add(room.m_type, room);
        }

        public void RemoveRoom(Room room)
        {
            if (!m_roomsByType.TryGetValue(room.m_type, out var currentRoom))
            {
                return;
            }
            if (currentRoom.m_id != room.m_id) return;

            m_roomsByType.Remove(room.m_type);
        }
        
        // todo: fusion probably
    }
}