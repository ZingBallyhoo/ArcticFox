using System;

namespace ArcticFox
{
    public interface ISpanConsumer<T>
    {
        void Input(ReadOnlySpan<T> input);
        void Abort();
    }
}