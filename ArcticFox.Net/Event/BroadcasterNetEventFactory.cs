using System;
using System.Diagnostics;
using ArcticFox.Codec;

namespace ArcticFox.Net.Event
{
    public class BroadcasterNetEventFactory : ISpanConsumer<byte>
    {
        public static BroadcasterNetEventFactory s_instance = new BroadcasterNetEventFactory();
        
        public void Input(ReadOnlySpan<byte> input, object? state)
        {
            var netEvent = NetEvent.Create(input);

            Debug.Assert(state != null);
            var broadcaster = (IBroadcaster) state;
            try
            {
                broadcaster.BroadcastEvent(netEvent);
            } catch (Exception)
            {
                // ignored, we must release our ref
            }

            netEvent.ReleaseCreationRef();
        }

        public void Abort()
        {
        }
    }
}