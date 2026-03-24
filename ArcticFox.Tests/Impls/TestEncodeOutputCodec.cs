using System;
using System.Collections.Generic;
using System.Text;
using ArcticFox.Codec;
using Xunit;

namespace ArcticFox.Tests.Impls
{
    public class TestEncodeOutputCodec : ISpanConsumer<byte>
    {
        public readonly List<string> m_outputAsStrings = new List<string>();

        public void Reset()
        {
            m_outputAsStrings.Clear();
        }

        public void Input(ReadOnlySpan<byte> input, ref object? state)
        {
            m_outputAsStrings.Add(Encoding.ASCII.GetString(input));
        }

        public void AssertOutput(params string[] expected)
        {
            Assert.Equal(expected, m_outputAsStrings);
            Reset();
        }
    }
}