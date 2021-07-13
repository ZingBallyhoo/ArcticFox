using System.Threading.Tasks;
using ArcticFox.Net.Event;

namespace ArcticFox.Net
{
    public static class BroadcasterExtensions
    {
        public static async ValueTask BroadcastEventOwningCreation<T>(this T bc, NetEvent ev) where T: IBroadcaster
        {
            try
            {
                await bc.BroadcastEvent(ev);
            } finally
            {
                ev.ReleaseCreationRef();
            }
        }
    }
}