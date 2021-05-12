using System;
using System.Collections.Generic;
using Xunit;

namespace ArcticFox.Tests
{
    public class TestOutputCodec : ISpanConsumer<char>
    {
        public List<string> m_received = new List<string>();
        public bool m_aborted;

        public void Reset()
        {
            m_received.Clear();
            m_aborted = false;
        }

        public void Input(ReadOnlySpan<char> input)
        {
            m_received.Add(input.ToString());
        }

        public void AssertReceived(params string[] expected)
        {
            Assert.False(m_aborted);
            Assert.Equal(expected, m_received);
            Reset();
        }

        public void AssertAborted()
        {
            Assert.True(m_aborted);
            Reset();
        }
        
        public void Abort()
        {
            // todo: how can i make this happen for text decoding issues etc...
            m_aborted = true;
        }
    }
}