using System;
using System.Text;

namespace ArcticFox.Tests
{
    public class TestCodecChain : IDisposable
    {
        private CodecChain m_chain;
        private TestOutputCodec m_output;

        public TestCodecChain(Encoding? encoding = null)
        {
            encoding ??= Encoding.ASCII;
            
            m_output = new TestOutputCodec();
            
            m_chain = new CodecChain();
            m_chain.AddCodec(new ZeroDelimitedCodec());
            m_chain.AddCodec(new TextCodec(encoding));
            m_chain.AddCodec(m_output);
        }

        public void DataInput(string str)
        {
            m_chain.Head<byte>().Input(Encoding.ASCII.GetBytes(str));
        }

        public void Dispose()
        {
            m_chain.Dispose();
        }

        public void AssertReceived(params string[] expected)
        {
            m_output.AssertReceived(expected);
        }

        public void AssertAborted()
        {
            m_output.AssertAborted();
        }
    }
}