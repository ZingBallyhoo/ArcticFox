using System;
using CommunityToolkit.HighPerformance.Buffers;

namespace ArcticFox.Codec
{
    public class ZeroDelimitedEncodeCodec : SpanCodec<char, char>
    {
        public override void Input(ReadOnlySpan<char> input, ref object? state)
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
                
                CodecOutput(input, ref state);
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
            CodecOutput(owner.Span, ref state);
        }
    }
}