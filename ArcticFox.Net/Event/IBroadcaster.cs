namespace ArcticFox.Net.Event
{
    public interface IBroadcaster
    {
        public void BroadcastEvent(NetEvent ev);
    }
}