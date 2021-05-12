using System;
using System.Text;
using CommunityToolkit.HighPerformance.Buffers;

namespace ArcticFox
{
    public class TextCodec : SpanCodec<byte, char>
    {
        private Decoder m_decoder;
        
        public TextCodec(Encoding encoding)
        {
            m_decoder = encoding.GetDecoder();
        }
        
        public override void Input(ReadOnlySpan<byte> buffer)
        {
            if (buffer.Length == 0)
            {
                Abort();
                return;
            }
            
            using var bufferOwner = SpanOwner<char>.Allocate(buffer.Length); // we will have at least len chars
            var charSpan = bufferOwner.Span;
            
            m_decoder.Convert(buffer, charSpan, true, out var bytesUsed, out var charCount, out var completed);

            if (!completed)
            {
                //Log.Information(
                //    "[TextReceiver:{Endpoint}]: Decoding was not finished after reading {CharCount} chars from a buffer of {BufferSize} bytes. {ConsumedBytes} bytes consumed",
                //    GetIdentifier(), charCount, buffer.Length, bytesUsed);
                Abort();
                return;
            }

            charSpan = charSpan.Slice(0, charCount);
            DecoderOutput(charSpan);
        }
    }
}