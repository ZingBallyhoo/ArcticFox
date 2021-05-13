using System;

namespace ArcticFox.Codec
{
    public abstract class SpanCodec<TFrom, TTo> : ISpanConsumer<TFrom>
    {
        public ISpanConsumer<TTo> m_next;
        
        public abstract void Input(ReadOnlySpan<TFrom> input, object? state);

        public void CodecOutput(ReadOnlySpan<TTo> output, object? state)
        {
            m_next.Input(output, state);
        }

        public virtual void Abort()
        {
            m_next.Abort();
        }
    }
}