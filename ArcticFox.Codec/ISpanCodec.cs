using System;

namespace ArcticFox.Codec
{
    public interface ISpanConsumer<T>
    {
        void Input(ReadOnlySpan<T> input, ref object? state);
    }
    
    public interface ISpanProducer<T>
    {
        public ISpanConsumer<T> Next { get; set; }
    }

    public interface ISpanCodec<TFrom, TTo> : ISpanConsumer<TFrom>, ISpanProducer<TTo>;

    public static class ISpanConsumerExtensions
    {
        public static void Input2<T>(this ISpanConsumer<T> i, ReadOnlySpan<T> input, object? state = null)
        {
            i.Input(input, ref state);
        }
    }
}