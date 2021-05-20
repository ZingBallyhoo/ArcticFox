using System;

namespace ArcticFox.Codec
{
    public abstract class SpanCodec<TFrom, TTo> : ISpanConsumer<TFrom>
    {
        public ISpanConsumer<TTo> m_next;
        
        public abstract void Input(ReadOnlyMemory<TFrom> input, ref object? state);

        public void CodecOutput(ReadOnlyMemory<TTo> output, ref object? state)
        {
            m_next.Input(output, ref state);
        }

        public virtual void Abort()
        {
            m_next.Abort();
        }
    }
}