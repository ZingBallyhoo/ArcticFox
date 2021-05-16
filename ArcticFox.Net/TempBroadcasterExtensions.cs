using System;
using System.Text;
using ArcticFox.Codec;
using ArcticFox.Net.Event;

namespace ArcticFox.Net
{
    public static class TempBroadcasterExtensions
    {
        private static readonly Lazy<CodecChain<char, byte>> s_zeroTerminatedStringCodec =
            new Lazy<CodecChain<char, byte>>(() =>
            {
                var chain = new CodecChain();
                chain.AddCodec(new ZeroDelimitedEncodeCodec());
                chain.AddCodec(new TextEncodeCodec(Encoding.ASCII));
                chain.AddCodec(BroadcasterNetEventFactory.s_instance);
                return new CodecChain<char, byte>(chain);
            });
        
        public static void Broadcast<T>(this T bc, ReadOnlySpan<char> msg) where T: IBroadcaster
        {
            s_zeroTerminatedStringCodec.Value.Input(msg, bc);
        }
        
        public static void Broadcast<T>(this T bc, ReadOnlySpan<byte> msg) where T: IBroadcaster
        {
            BroadcasterNetEventFactory.s_instance.Input(msg, bc);
        }
    }
}