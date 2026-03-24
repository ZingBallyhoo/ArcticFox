using System;

namespace ArcticFox.Codec
{
    public class CodecChainBuilder<THead, TCurrent> : ISpanCodec<THead, TCurrent>
    {
        private readonly CodecChain<THead> m_chain;
        private readonly ISpanProducer<TCurrent> m_current;

        // populated if passed to ChainTo ourselves, instead of finishing the chain normally
        public ISpanConsumer<TCurrent> Next
        {
            get => m_current.Next;
            set => m_current.Next = value;
        }

        public void Input(ReadOnlySpan<THead> input, ref object? state)
        {
            m_chain.Input(input, ref state);
        }
        
        public CodecChainBuilder(ISpanCodec<THead, TCurrent> head)
        {
            m_chain = new CodecChain<THead>(head);
            m_current = head;
        }

        private CodecChainBuilder(CodecChain<THead> chain, ISpanProducer<TCurrent> current)
        {
            m_chain = chain;
            m_current = current;
        }

        public CodecChainBuilder<THead, TNext> ChainTo<TNext>(ISpanCodec<TCurrent, TNext> next)
        {
            var builder = ChainTo((ISpanConsumer<TCurrent>)next);

            return new CodecChainBuilder<THead, TNext>(builder, next);
        }
        
        public CodecChain<THead> ChainTo(ISpanConsumer<TCurrent> next)
        {
            m_chain.RegisterDisposable(next);
            m_current.Next = next;

            return m_chain;
        }
    }
    
    public static class CodecChainBuilderExtensions
    {
        public static CodecChainBuilder<THead, TTo> ChainTo<THead, TFrom, TTo>(this ISpanCodec<THead, TFrom> first, ISpanCodec<TFrom, TTo> next)
        {
            var builder = new CodecChainBuilder<THead, TFrom>(first);
            return builder.ChainTo(next);
        }
        
        public static CodecChain<THead> ChainTo<THead, TFrom>(this ISpanCodec<THead, TFrom> first, ISpanConsumer<TFrom> next)
        {
            var builder = new CodecChainBuilder<THead, TFrom>(first);
            return builder.ChainTo(next);
        }
        
        /*public static CodecChainBuilder<THead, TTo> ChainTo<THead, TFrom, TTo>(this CodecChainBuilder<THead, TFrom> previous, ISpanCodec<TFrom, TTo> next)
        {
            return previous.ChainTo(next);
        }
        
        public static CodecChain<THead> ChainTo<THead, TFrom>(this CodecChainBuilder<THead, TFrom> previous, ISpanConsumer<TFrom> next)
        {
            return previous.ChainTo(next);
        }*/
    }
}