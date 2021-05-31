using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stl.Async;
using Stl.Fusion;

namespace ArcticFox.SmartFoxServer
{
    [RegisterComputeService(Lifetime = ServiceLifetime.Transient)]
    public class RoomCollection
    {
        public readonly Dictionary<ulong, Room> m_roomsByID = new Dictionary<ulong, Room>();
        public readonly Dictionary<string, Room> m_roomsByName = new Dictionary<string, Room>();

        public void AddRoom(Room room)
        {
            if (m_roomsByID.ContainsKey(room.m_id) || m_roomsByName.ContainsKey(room.m_name))
            {
                throw new ArgumentException();
            }
            
            m_roomsByName.Add(room.m_name, room); // add room first, its more likely to be duplicate
            m_roomsByID.Add(room.m_id, room);
            
            using (Computed.Invalidate())
            {
                GetByName(room.m_name).Ignore();
            }
        }

        public void RemoveRoom(Room room)
        {
            m_roomsByName.Remove(room.m_name);
            m_roomsByID.Remove(room.m_id);
            
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
}