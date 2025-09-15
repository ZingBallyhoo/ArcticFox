using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ArcticFox.Codec
{
    public class CodecChain : IDisposable
    {
        private readonly List<IDisposable> m_disposableCodecs;

        private object? m_head;
        private object? m_tail;
        
        public CodecChain()
        {
            m_disposableCodecs = new List<IDisposable>();
        }

        public ISpanConsumer<T> Head<T>()
        {
            Debug.Assert(m_head != null);
            return (ISpanConsumer<T>)m_head;
        }

        public CodecChain AddCodec<TTo>(ISpanConsumer<TTo> next)
        {
            if (m_head == null) m_head = next;
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

        public void Dispose()
        {
            foreach (var codec in m_disposableCodecs)
            {
                codec.Dispose();
            }
        }
    }

    public class CodecChain<TFrom, TTo> : SpanCodec<TFrom, TTo>, IDisposable
    {
        private readonly CodecChain m_chain;
        
        public CodecChain(CodecChain chain)
        {
            m_chain = chain;
        }

        public override void Input(ReadOnlySpan<TFrom> input, ref object? state)
        {
            m_chain.Head<TFrom>().Input(input, ref state);
        }

        public void Dispose()
        {
            m_chain.Dispose();
        }
    }
}