using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ArcticFox.Net.Sockets;
using Xunit;

namespace ArcticFox.Tests.Impls
{
    public class TestSocketInterface : SocketInterface
    {
        public List<string> m_sentAsStrings = new List<string>();
        
        public override Task SendBuffer(ReadOnlyMemory<byte> data)
        {
            m_sentAsStrings.Add(Encoding.ASCII.GetString(data.Span));
            return Task.CompletedTask;
        }

        public override Task<int> ReceiveBuffer(Memory<byte> buffer)
        {
            throw new NotImplementedException();
        }

        protected override Task CloseSocket()
        {
            throw new NotImplementedException();
        }
        
        public void AssertSent(params string[] expected)
        {
            Assert.Equal(expected, m_sentAsStrings);
            m_sentAsStrings.Clear();
        }
    }
}