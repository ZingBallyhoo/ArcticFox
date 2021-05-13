using System;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;

namespace ArcticFox.Codec
{
    public class ZeroDelimitedEncodeCodec : SpanCodec<char, char>
    {
        public static readonly ZeroDelimitedEncodeCodec s_instance = new ZeroDelimitedEncodeCodec();
        
        public override void Input(ReadOnlySpan<char> input, object? state)
        {
            var countOf0 = input.Count('\0');
            if (input[^1] == '\0')
            {
                if (countOf0 != 1)
                {
                    // it should only be the last char
                    Abort();
                    return;
                }
                
                CodecOutput(input, state);
                return;
            }
            
            if (countOf0 != 0)
            {
                // it wasn't the last char, so its hiding in the data
                Abort();
                return;
            }
            
            using var owner = SpanOwner<char>.Allocate(input.Length + 1);
            input.CopyTo(owner.Span);
            owner.Span[input.Length] = '\0';
            CodecOutput(owner.Span, state);
        }
    }
}