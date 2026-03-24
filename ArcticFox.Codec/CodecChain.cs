using System;
using System.Collections.Generic;

namespace ArcticFox.Codec
{
    public class CodecChain<THead> : ISpanConsumer<THead>, IDisposable
    {
        private readonly ISpanConsumer<THead> m_head;
        private readonly List<IDisposable> m_disposableCodecs = [];

        public CodecChain(ISpanConsumer<THead> head)
        {
            m_head = head;
            RegisterDisposable(head);
        }
        
        public void Input(ReadOnlySpan<THead> input, ref object? state)
        {
            m_head.Input(input, ref state);
        }

        internal void RegisterDisposable(object codec)
        {
            if (codec is IDisposable disposable)
            {
                m_disposableCodecs.Add(disposable);
            }
        }

        public void Dispose()
        {
            foreach (var disposable in m_disposableCodecs)
            {
                disposable.Dispose();
            }
            
            GC.SuppressFinalize(this);
        }
    }
}