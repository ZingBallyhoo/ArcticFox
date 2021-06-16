using System.Collections.Generic;
using ArcticFox.Net.Util;

namespace ArcticFox.SmartFoxServer
{
    public static class RoomTypeIDs
    {
        private static readonly IDFactory s_typeIdFactory = new IDFactory();
        private static readonly Dictionary<int, string> s_names = new Dictionary<int, string>
        {
            {0, "DefaultRoomType"}
        };
        private static readonly Dictionary<int, IDFactory> s_defaultNameFactories = new Dictionary<int, IDFactory>
        {
            {0, new IDFactory()}
        };

        public const int DEFAULT = 0;

        public static int Next(string name)
        {
            var id = (int)s_typeIdFactory.Next();
            s_names[id] = name;
            s_defaultNameFactories[id] = new IDFactory();
            return id;
        }

        public static string GenerateRoomName(int typeID)
        {
            var typeName = GetName(typeID);
            var idFactory = s_defaultNameFactories[typeID];
            return $"{typeName}_{idFactory.Next()}";
        }

        public static string GetName(int typeID)
        {
            s_names.TryGetValue(typeID, out var name);
            name ??= $"Unknown{typeID}";
            return name;
        }
    }
}