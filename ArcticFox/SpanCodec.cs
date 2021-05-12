using System;

namespace ArcticFox
{
    public abstract class SpanCodec<TFrom, TTo> : ISpanConsumer<TFrom>
    {
        public ISpanConsumer<TTo> m_next;
        
        public abstract void Input(ReadOnlySpan<TFrom> input);

        public void CodecOutput(ReadOnlySpan<TTo> output)
        {
            m_next.Input(output);
        }

        public virtual void Abort()
        {
            m_next.Abort();
        }
    }
}