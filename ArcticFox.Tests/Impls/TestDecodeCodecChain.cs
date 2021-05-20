using System;
using System.Text;
using ArcticFox.Codec;

namespace ArcticFox.Tests.Impls
{
    public class TestDecodeCodecChain : IDisposable
    {
        private CodecChain m_chain;
        private TestDecodeOutputCodec m_output;

        public TestDecodeCodecChain(Encoding? encoding = null)
        {
            encoding ??= Encoding.ASCII;
            
            m_output = new TestDecodeOutputCodec();
            
            m_chain = new CodecChain();
            m_chain.AddCodec(new ZeroDelimitedDecodeCodec());
            m_chain.AddCodec(new TextDecodeCodec(encoding));
            m_chain.AddCodec(m_output);
        }

        public void DataInput(string str)
        {
            m_chain.Head<byte>().Input2(Encoding.ASCII.GetBytes(str));
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