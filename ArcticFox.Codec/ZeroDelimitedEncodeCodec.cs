using System;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;

namespace ArcticFox.Codec
{
    public class ZeroDelimitedEncodeCodec : SpanCodec<char, char>
    {
        public static readonly ZeroDelimitedEncodeCodec s_instance = new ZeroDelimitedEncodeCodec();
        
        public override void Input(ReadOnlyMemory<char> input, ref object? state)
        {
            var inputSpan = input.Span;
            var countOf0 = inputSpan.Count('\0');
            if (inputSpan[^1] == '\0')
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
            
            using var owner = MemoryOwner<char>.Allocate(input.Length + 1);
            inputSpan.CopyTo(owner.Span);
            owner.Span[input.Length] = '\0';
            CodecOutput(owner.Memory, ref state);
        }
    }
}