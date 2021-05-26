using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArcticFox.Net;
using ArcticFox.Net.Util;
using Microsoft.Extensions.DependencyInjection;
using Stl.Async;
using Stl.Fusion;

namespace ArcticFox.SmartFoxServer
{
    [RegisterComputeService(Lifetime = ServiceLifetime.Singleton)]
    public class SmartFoxManager
    {
        public readonly IServiceProvider m_provider;
        private readonly AsyncLockedAccess<Dictionary<string, Zone>> m_zones;

        public SmartFoxManager(IServiceProvider provider)
        {
            m_provider = provider;
            m_zones = new AsyncLockedAccess<Dictionary<string, Zone>>(new Dictionary<string, Zone>());
        }

        public async ValueTask<User> CreateUser(string name, HighLevelSocket? socket, string zoneName)
        {
            var zone = await GetZone(zoneName);
            if (zone == null) throw new ArgumentException($"{nameof(CreateUser)}: zone {zoneName} doesn't exist");
            
            using var token = await m_zones.Get();
            foreach (var zoneToCheck in token.m_value.Values)
            {
                var userInZone = await zoneToCheck.GetUser(name);
                if (userInZone == null) continue;
                throw new ArgumentException($"{nameof(CreateUser)}: user {name} is already logged into {zoneToCheck.m_name}");
            }

            var user = await zone.CreateUser(name, socket);
            return user;
        }

        public async ValueTask LogoutSocket(HighLevelSocket socket)
        {
            using var token = await m_zones.Get();
            var zones = token.m_value;

            foreach (var zone in zones.Values)
            {
                var user = await zone.GetUser(socket);
                if (user == null) continue;
                await zone.RemoveUser(user);
            }
        }
        
        public async ValueTask<Zone> CreateZone(string name)
        {
            using var token = await m_zones.Get();
            var zones = token.m_value;
            if (zones.ContainsKey(name)) throw new ArgumentException($"{nameof(CreateZone)}: zone {name} already exists");
            
            var scope = m_provider.CreateScope();
            var definition = scope.ServiceProvider.GetRequiredService<ZoneDefinition>();
            definition.m_name = name;
            definition.m_scope = scope;
            var zone = scope.ServiceProvider.GetRequiredService<Zone>();

            zones[name] = zone;

            using (Computed.Invalidate())
            {
                GetZone(name).AssertCompleted().Ignore();
            }
            
            return zone;
        }

        [ComputeMethod]
        public virtual async ValueTask<Zone?> GetZone(string name)
        {
            using var token = await m_zones.Get();
            token.m_value.TryGetValue(name, out var zone);
            return zone;
        }
    }
}