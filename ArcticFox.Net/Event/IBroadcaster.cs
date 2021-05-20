using System.Threading.Tasks;

namespace ArcticFox.Net.Event
{
    public interface IBroadcaster
    {
        public ValueTask BroadcastEvent(NetEvent ev);
    }
}