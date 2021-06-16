using System.Threading.Tasks;

namespace ArcticFox.SmartFoxServer
{
    public interface IRoomEventHandler
    {
        ValueTask UserJoinedRoom(Room room, User user) => ValueTask.CompletedTask;
        ValueTask UserLeftRoom(Room room, User user) => ValueTask.CompletedTask;
    }
}