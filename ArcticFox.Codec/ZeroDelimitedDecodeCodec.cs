using System;

namespace ArcticFox.Codec
{
    public class ZeroDelimitedDecodeCodec : DynamicSizeBufferCodec<byte>
    {
        public byte m_delimitByte = 0;

        public override void Input(ReadOnlySpan<byte> input, ref object? state)
        {
            while (input.Length > 0)
            {
                var idxOf0 = input.IndexOf(m_delimitByte);

                if (idxOf0 == -1)
                {
                    ExtendMemory(ref input);
                } else
                {
                    ExtendFinalMemory(ref input, input.Slice(0, idxOf0), ref state);
                    input = input.Slice(1); // skip 0
                }
            }
        }

        protected override void Reset()
        {
        }
    }
}