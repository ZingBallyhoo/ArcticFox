using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ArcticFox.Tests
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

        public void Input(ReadOnlySpan<byte> input)
        {
            m_outputAsStrings.Add(Encoding.ASCII.GetString(input));
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