using System;

namespace ArcticFox.Codec
{
    public interface ISpanConsumer<T>
    {
        void Input(ReadOnlySpan<T> input, object? state);
        void Abort();
    }
}