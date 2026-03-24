using System;
using System.Collections.Generic;
using ArcticFox.Codec;
using Xunit;

namespace ArcticFox.Tests.Impls
{
    public class TestDecodeOutputCodec : ISpanConsumer<char>
    {
        public readonly List<string> m_received = new List<string>();

        public void Reset()
        {
            m_received.Clear();
        }

        public void Input(ReadOnlySpan<char> input, ref object? state)
        {
            m_received.Add(input.ToString());
        }

        public void AssertReceived(params string[] expected)
        {
            Assert.Equal(expected, m_received);
            Reset();
        }
    }
}