using System;
using System.Text;
using CommunityToolkit.HighPerformance.Buffers;

namespace ArcticFox.Codec
{
    public class TextEncodeCodec : SpanCodec<char, byte>
    {
        private readonly Encoding m_encoding;
        private readonly Encoder m_encoder;
        
        public TextEncodeCodec(Encoding encoding)
        {
            m_encoding = encoding;
            m_encoder = encoding.GetEncoder();
        }
        
        public override void Input(ReadOnlySpan<char> input, ref object? state)
        {
            var maxBytes = m_encoding.GetMaxByteCount(input.Length);
            using var buffer = SpanOwner<byte>.Allocate(maxBytes);
            m_encoder.Convert(input, buffer.Span, true, out var charsUsed, out var bytesUsed, out var completed);
            if (!completed) throw new Exception();

            var usedData = buffer.Span.Slice(0, bytesUsed);
            CodecOutput(usedData, ref state);
        }
    }
}