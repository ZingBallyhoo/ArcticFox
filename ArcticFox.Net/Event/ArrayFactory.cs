using System;
using ArcticFox.Codec;

namespace ArcticFox.Net.Event
{
    public class ArrayFactory<T> : ISpanConsumer<T>
    {
        public static readonly ArrayFactory<T> s_instance = new ArrayFactory<T>();

        public void Input(ReadOnlySpan<T> input, ref object? state)
        {
            state = input.ToArray();
        }
        
        public void Abort()
        {
        }
    }
}