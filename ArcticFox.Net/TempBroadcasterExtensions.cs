using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
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
                chain.AddCodec(NetEventFactory.s_instance);
                return new CodecChain<char, byte>(chain);
            });
        
        public static ValueTask BroadcastZeroTerminatedAscii<T>(this T bc, ReadOnlySpan<char> msg) where T: IBroadcaster
        {
            object? ev = null;
            s_zeroTerminatedStringCodec.Value.Input(msg, ref ev);
            Debug.Assert(ev != null);
            var netEv = (NetEvent) ev;
            return bc.BroadcastEventOwningCreation(netEv);
        }

        public static ValueTask BroadcastBytes<T>(this T bc, ReadOnlySpan<byte> msg) where T: IBroadcaster
        {
            object? ev = null;
            NetEventFactory.s_instance.Input(msg, ref ev);
            Debug.Assert(ev != null);
            var netEv = (NetEvent) ev;
            return bc.BroadcastEventOwningCreation(netEv);
        }
    }
}