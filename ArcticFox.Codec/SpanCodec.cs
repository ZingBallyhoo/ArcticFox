using System;

namespace ArcticFox.Codec
{
    public abstract class SpanCodec<TFrom, TTo> : ISpanCodec<TFrom, TTo>
    {
        public ISpanConsumer<TTo> Next { get; set; } = null!;
        
        public abstract void Input(ReadOnlySpan<TFrom> input, ref object? state);

        protected void CodecOutput(ReadOnlySpan<TTo> output, ref object? state)
        {
            Next.Input(output, ref state);
        }
    }
}