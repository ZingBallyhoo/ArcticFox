using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ArcticFox
{
    public class CodecChain : IDisposable
    {
        private List<IDisposable> m_disposableCodecs;

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

        public CodecChain AddCodec(object next)
        {
            if (m_head == null) m_head = next;
            if (m_tail != null)
            {
                // todo: reflection in 2021 oh no
                var nextField = m_tail.GetType().GetField("m_next");
                Debug.Assert(nextField != null);
                nextField.SetValue(m_tail, next);
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
}