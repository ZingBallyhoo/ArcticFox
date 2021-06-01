using System.Threading.Tasks;

namespace ArcticFox.SmartFoxServer
{
    public interface ISystemHandler : IRoomEventHandler
    {
        ValueTask RoomCreated(Room room) => ValueTask.CompletedTask;
        ValueTask RoomRemoved(Room room) => ValueTask.CompletedTask;
    }

    public class NullSystemHandler : ISystemHandler
    {
    }
}