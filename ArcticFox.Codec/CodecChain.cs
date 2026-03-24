using System;
using System.Collections.Generic;

namespace ArcticFox.Codec
{
    public class CodecChain<T> : ISpanConsumer<T>, IDisposable
    {
        private readonly List<IDisposable> m_disposableCodecs = [];

        private ISpanConsumer<T>? m_head;
        private object? m_tail;
        
        public CodecChain<T> AddCodec<TTo>(ISpanConsumer<TTo> next)
        {
            m_head ??= (ISpanConsumer<T>)next;
            if (m_tail != null)
            {
                var tailChain = (SpanCodecBase<TTo>)m_tail;
                tailChain.m_next = next;
            }
            m_tail = next;
            
            if (next is IDisposable disposable)
            {
                m_disposableCodecs.Add(disposable);
            }
            
            return this;
        }

        public void Input(ReadOnlySpan<T> input, ref object? state)
        {
            m_head!.Input(input, ref state);
        }

        public void Dispose()
        {
            foreach (var codec in m_disposableCodecs)
            {
                codec.Dispose();
            }
        }
    }
}