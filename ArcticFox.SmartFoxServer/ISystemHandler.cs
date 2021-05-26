using System.Threading.Tasks;

namespace ArcticFox.SmartFoxServer
{
    public interface ISystemHandler
    {
        ValueTask RoomCreated(Room room) => ValueTask.CompletedTask;
        ValueTask RoomRemoved(Room room) => ValueTask.CompletedTask;
        ValueTask UserJoinedRoom(Room room, User user) => ValueTask.CompletedTask;
        ValueTask UserLeftRoom(Room room, User user) => ValueTask.CompletedTask;
    }

    public class NullSystemHandler : ISystemHandler
    {
    }
}