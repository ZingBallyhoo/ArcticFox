using System;
using System.Threading.Tasks;
using ArcticFox.Net;
using ArcticFox.Net.Sockets;
using ArcticFox.SmartFoxServer;
using Microsoft.Extensions.DependencyInjection;
using Stl.DependencyInjection;
using Xunit;

namespace ArcticFox.Tests.SmartFoxServer
{
    public class SmartFoxSocketBase : HighLevelSocket
    {
        protected readonly SmartFoxManager m_manager;
        
        public SmartFoxSocketBase(SocketInterface socket, SmartFoxManager manager) : base(socket)
        {
            m_manager = manager;
        }

        public async ValueTask<User> CreateUser(string zoneName, string name)
        {
            var zone = await m_manager.GetZone(zoneName);
            if (zone == null) throw new ArgumentException($"{nameof(CreateUser)}: zone {zoneName} doesn't exist");
            var user = await zone.CreateUser(name, this);
            return user;
        }
    }

    public class NullSocketInterface : SocketInterface
    {
        public override ValueTask SendBuffer(ReadOnlyMemory<byte> data)
        {
            throw new NotImplementedException();
        }

        public override ValueTask<int> ReceiveBuffer(Memory<byte> buffer)
        {
            throw new NotImplementedException();
        }

        protected override ValueTask CloseSocket()
        {
            throw new NotImplementedException();
        }
    }
    
    public class TestSocket : HighLevelSocket
    {
        public TestSocket(SocketInterface socket) : base(socket)
        { }
    }
    
    public class ZoneTests
    {
        private SmartFoxManager CreateMgr()
        {
            var services = new ServiceCollection();
            services.UseRegisterAttributeScanner().RegisterFrom(typeof(Zone).Assembly);
            services.AddSingleton<ISystemHandler, NullSystemHandler>();

            var provider = services.BuildServiceProvider();
            var mgr = provider.GetRequiredService<SmartFoxManager>();
            return mgr;
        }

        public async ValueTask<Zone> CreateZone(SmartFoxManager mgr, string name)
        {
            var zone = await mgr.CreateZone(name);
            Assert.Equal(zone.m_name, name);
            return zone;
        }
        
        [Fact]
        public async Task Test1()
        {
            var mgr = CreateMgr();
            
            using var zone1 = await CreateZone(mgr, "zone");
            Assert.NotNull(zone1);
            var room1 = await zone1.CreateRoom("room1");
            Assert.Same(room1.m_zone, zone1);
            var room2 = await zone1.CreateRoom("room2");
            Assert.Same(room2.m_zone, zone1);
            Assert.NotSame(room1, room2);
            Assert.NotEqual(room1.m_id, room2.m_id);
            await Assert.ThrowsAsync<ArgumentException>(async () => await zone1.CreateRoom("room2"));

            using var zone2 = await CreateZone(mgr, "zone2");
            Assert.NotNull(zone2);
            Assert.NotSame(zone2, zone1);
            var userA = await zone2.CreateUser("a", null);
            var userB = await zone2.CreateUser("b", null);
            
            Assert.Equal("a", userA.m_name);
            Assert.Equal("b", userB.m_name);
            Assert.NotSame(userA, userB);
            Assert.NotEqual(userA.m_id, userB.m_id);

            await Assert.ThrowsAsync<ArgumentException>(async () => await zone2.CreateUser("b", null));
        }

        [Fact] 
        public async Task TestTempRoom()
        {
            var mgr = CreateMgr();
            using var zone = await CreateZone(mgr, "zone");

            const string roomName = "user_house";
            
            var user = await zone.CreateUser("user", new TestSocket(new NullSocketInterface()));
            var room = await zone.CreateRoom(new RoomDescription
            {
                m_name = roomName,
                m_creator = user,
                m_isTemporary = true
            });

            var roomGet1 = await zone.GetRoom(roomName);
            Assert.Same(roomGet1, room);

            await zone.RemoveUser(user);
            var roomGet2 = await zone.GetRoom(roomName);
            Assert.Null(roomGet2);
        }
        
        [Fact] 
        public async Task TestTempRoomUntilEmpty()
        {
            var mgr = CreateMgr();
            using var zone = await CreateZone(mgr, "zone");

            const string roomName = "user1_house";
            const string persistentRoomName = "hub";
            
            var user1 = await zone.CreateUser("user1", null);
            var user2 = await zone.CreateUser("user2", null);
            var room = await zone.CreateRoom(new RoomDescription
            {
                m_name = roomName,
                m_creator = user1,
                m_isTemporary = true
            });
            Assert.NotNull(room);
            var persistentRoom = await zone.CreateRoom(new RoomDescription
            {
                m_name = persistentRoomName
            });

            var roomGet1 = await zone.GetRoom(roomName);
            Assert.Same(roomGet1, room);

            await persistentRoom.AddUser(user1);
            await persistentRoom.RemoveUser(user1);

            await room.AddUser(user1);
            await room.AddUser(user2);
            
            await room.RemoveUser(user1);
            await room.RemoveUser(user2);
            
            await room.AddUser(user1);
            await room.AddUser(user2);
            
            await zone.RemoveUser(user1);
            
            var roomGet2 = await zone.GetRoom(roomName);
            Assert.Same(roomGet2, room);

            await room.RemoveUser(user2);
            
            var roomGet3 = await zone.GetRoom(roomName);
            Assert.Null(roomGet3);

            await Assert.ThrowsAsync<Exception>(async () => await roomGet2.AddUser(user2));
            
            await zone.RemoveUser(user2);
            
            var persistentRoomGet = await zone.GetRoom(persistentRoomName);
            Assert.Same(persistentRoom, persistentRoomGet);
            
            await Assert.ThrowsAsync<Exception>(async () => await persistentRoom.AddUser(user2));
        }

        [Fact]
        public async Task CantCreateSameZoneTwice()
        {
            var mgr = CreateMgr();
            using var zone = await CreateZone(mgr, "zone");

            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                using var zoneAgain = await mgr.CreateZone("zone");
            });
        }
        
        [Fact]
        public async Task CantLogIntoTwoZones()
        {
            var mgr = CreateMgr();
            using var zone1 = await CreateZone(mgr, "zone");
            using var zone2 = await CreateZone(mgr, "zone2");

            await mgr.CreateUser("bob", null, "zone");

            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await mgr.CreateUser("bob", null, "zone2");
            });
        }
    }
}
