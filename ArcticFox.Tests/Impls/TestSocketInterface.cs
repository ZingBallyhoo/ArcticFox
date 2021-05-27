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
        
        public override ValueTask SendBuffer(ReadOnlyMemory<byte> data)
        {
            m_sentAsStrings.Add(Encoding.ASCII.GetString(data.Span));
            return ValueTask.CompletedTask;
        }

        public override async ValueTask<int> ReceiveBuffer(Memory<byte> buffer)
        {
            await Task.Delay(-1, m_cancellationTokenSource.Token);
            return 0; // unreachable
        }

        protected override ValueTask CloseSocket()
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