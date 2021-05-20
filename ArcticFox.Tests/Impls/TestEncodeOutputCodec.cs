using System;
using System.Collections.Generic;
using System.Text;
using ArcticFox.Codec;
using Xunit;

namespace ArcticFox.Tests.Impls
{
    public class TestEncodeOutputCodec : ISpanConsumer<byte>
    {
        public List<string> m_outputAsStrings = new List<string>();
        public bool m_aborted;

        public void Reset()
        {
            m_outputAsStrings.Clear();
            m_aborted = false;
        }

        public void Input(ReadOnlyMemory<byte> input, ref object? state)
        {
            m_outputAsStrings.Add(Encoding.ASCII.GetString(input.Span));
        }

        public void AssertOutput(params string[] expected)
        {
            Assert.False(m_aborted);
            Assert.Equal(expected, m_outputAsStrings);
            Reset();
        }

        public void AssertAborted()
        {
            Assert.True(m_aborted);
            Reset();
        }
        
        public void Abort()
        {
            m_aborted = true;
        }
    }
}