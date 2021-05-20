using System;

namespace ArcticFox.Codec
{
    public interface ISpanConsumer<T>
    {
        void Input(ReadOnlyMemory<T> input, ref object? state);
        void Abort();
    }

    public static class ISpanConsumerExtensions
    {
        public static void Input2<T>(this ISpanConsumer<T> i, ReadOnlyMemory<T> input, object? state = null)
        {
            i.Input(input, ref state);
        }
    }
}