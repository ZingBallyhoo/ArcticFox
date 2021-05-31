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
    public class TestSFSSocketHost : SocketHost
    {
        private readonly SmartFoxManager m_mgr;
        
        public TestSFSSocketHost(SmartFoxManager manager)
        {
            m_mgr = manager;
        }
        
        public override HighLevelSocket CreateHighLevelSocket(SocketInterface socket)
        {
            return new SmartFoxSocketBase(socket, m_mgr);
        }
    }

    public class NullSocketInterface : SocketInterface
    {
        public override ValueTask SendBuffer(ReadOnlyMemory<byte> data)
        {
            throw new NotImplementedException();
        }

        public override async ValueTask<int> ReceiveBuffer(Memory<byte> buffer)
        {
            await Task.Delay(-1, m_cancellationTokenSource.Token);
            return 0; // unreachable
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

            await user1.MoveTo(persistentRoom);
            await user1.RemoveFromRoom(RoomTypeIDs.DEFAULT);

            await user1.MoveTo(room);
            await user2.MoveTo(room);

            await user1.RemoveFromRoom(RoomTypeIDs.DEFAULT);
            await user2.RemoveFromRoom(RoomTypeIDs.DEFAULT);
            
            await user1.MoveTo(room);
            await user2.MoveTo(room);
            
            await zone.RemoveUser(user1);
            
            var roomGet2 = await zone.GetRoom(roomName);
            Assert.Same(roomGet2, room);

            await user2.RemoveFromRoom(RoomTypeIDs.DEFAULT);
            
            var roomGet3 = await zone.GetRoom(roomName);
            Assert.Null(roomGet3);

            await Assert.ThrowsAsync<Exception>(async () => await user2.MoveTo(roomGet2));
            
            await zone.RemoveUser(user2);
            
            var persistentRoomGet = await zone.GetRoom(persistentRoomName);
            Assert.Same(persistentRoom, persistentRoomGet);
            
            await Assert.ThrowsAsync<Exception>(async () => await user2.MoveTo(persistentRoom));
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
        
        [Fact]
        public async Task SocketShutdownLogsOutUser()
        {
            const string userName = "bob";
            
            var mgr = CreateMgr();
            using var zone = await CreateZone(mgr, "zone");
            var room = await zone.CreateRoom(new RoomDescription
            {
                m_name = "the room"
            });
            
            await using var host = new TestSFSSocketHost(mgr);
            await host.StartAsync();

            var socket = (SmartFoxSocketBase)host.CreateHighLevelSocket(new NullSocketInterface());
            await host.AddSocket(socket);
            var user = await socket.CreateUser("zone", userName);
            Assert.NotNull(user);

            await user.MoveTo(room);
            
            user.m_socket!.Close();
            await Task.Delay(300);

            Assert.Null(await zone.GetUser(userName));
        }

        [Fact]
        public async Task DuplicateOwnedRoomDoesntDeleteOriginal()
        {
            var mgr = CreateMgr();
            using var zone = await CreateZone(mgr, "zone");

            const string ownerName = "room owner";
            const string roomName = "owned room";
            
            var user = await zone.CreateUser(ownerName, null);
            var room = await zone.CreateRoom(new RoomDescription
            {
                m_name = roomName,
                m_creator = user,
                m_isTemporary = true
            });
            
            var otherUser = await zone.CreateUser("other user", null);
            await otherUser.MoveTo(room);

            await zone.RemoveUser(user);
            
            // room stays alive cos the other user is in it
            Assert.NotNull(await zone.GetRoom(roomName));

            var userAgain = await zone.CreateUser(ownerName, null);
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await zone.CreateRoom(new RoomDescription
                {
                    m_name = roomName,
                    m_creator = userAgain,
                    m_isTemporary = true
                });
            });
            await zone.RemoveUser(userAgain);

            // user error'd logging in but the existing room should still exist
            Assert.NotNull(await zone.GetRoom(roomName));
        }
    }
}
