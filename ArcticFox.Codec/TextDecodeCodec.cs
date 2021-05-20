using System;
using System.Text;
using CommunityToolkit.HighPerformance.Buffers;

namespace ArcticFox.Codec
{
    public class TextDecodeCodec : SpanCodec<byte, char>
    {
        private readonly Encoding m_encoding;
        private readonly Decoder m_decoder;
        
        public TextDecodeCodec(Encoding encoding)
        {
            m_encoding = encoding;
            m_decoder = encoding.GetDecoder();
        }
        
        public override void Input(ReadOnlyMemory<byte> buffer, ref object? state)
        {
            if (buffer.Length == 0)
            {
                Abort();
                return;
            }

            var maxChars = m_encoding.GetMaxCharCount(buffer.Length);
            using var bufferOwner = MemoryOwner<char>.Allocate(maxChars);
            var charSpan = bufferOwner.Span;
            
            m_decoder.Convert(buffer.Span, charSpan, true, out var bytesUsed, out var charCount, out var completed);

            if (!completed)
            {
                //Log.Information(
                //    "[TextReceiver:{Endpoint}]: Decoding was not finished after reading {CharCount} chars from a buffer of {BufferSize} bytes. {ConsumedBytes} bytes consumed",
                //    GetIdentifier(), charCount, buffer.Length, bytesUsed);
                Abort();
                return;
            }

            var charMemory = bufferOwner.Memory;
            charMemory = charMemory.Slice(0, charCount);
            CodecOutput(charMemory, ref state);
        }
    }
}