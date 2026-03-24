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
        private static readonly Lazy<CodecChain<char>> s_zeroTerminatedStringCodec =
            new Lazy<CodecChain<char>>(() =>
            {
                var chain = new CodecChain<char>();
                chain.AddCodec(new ZeroDelimitedEncodeCodec());
                chain.AddCodec(new TextEncodeCodec(Encoding.ASCII));
                chain.AddCodec(NetEventFactory.s_instance);
                return chain;
            });
        
        private static readonly Lazy<CodecChain<char>> s_zeroTerminatedStringToBytesCodec =
            new Lazy<CodecChain<char>>(() =>
            {
                var chain = new CodecChain<char>();
                chain.AddCodec(new ZeroDelimitedEncodeCodec());
                chain.AddCodec(new TextEncodeCodec(Encoding.ASCII));
                chain.AddCodec(ArrayFactory<byte>.s_instance);
                return chain;
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
        
        public static byte[] SerializeZeroTerminatedAscii(ReadOnlySpan<char> msg)
        {
            object? state = null;
            s_zeroTerminatedStringToBytesCodec.Value.Input(msg, ref state);
            Debug.Assert(state != null);
            
            return (byte[])state;
        }
    }
}