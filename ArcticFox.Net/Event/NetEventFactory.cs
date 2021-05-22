using System;
using ArcticFox.Codec;

namespace ArcticFox.Net.Event
{
    public class NetEventFactory : ISpanConsumer<byte>
    {
        public static NetEventFactory s_instance = new NetEventFactory();
        
        public void Input(ReadOnlySpan<byte> input, ref object? state)
        {
            var netEvent = NetEvent.Create(input);
            state = netEvent;
        }

        public void Abort()
        {
        }
    }
}