using System;
using System.Text;
using ArcticFox.Codec;

namespace ArcticFox.Tests.Impls
{
    public class TestDecodeCodecChain : IDisposable
    {
        private CodecChain<byte> m_chain;
        private TestDecodeOutputCodec m_output;

        public TestDecodeCodecChain(Encoding? encoding = null)
        {
            encoding ??= Encoding.ASCII;
            
            m_output = new TestDecodeOutputCodec();
            
            m_chain = new ZeroDelimitedDecodeCodec()
                .ChainTo(new TextDecodeCodec(encoding))
                .ChainTo(m_output);
        }

        public void DataInput(string str)
        {
            m_chain.Input2(Encoding.ASCII.GetBytes(str));
        }

        public void Dispose()
        {
            m_chain.Dispose();
        }

        public void AssertReceived(params string[] expected)
        {
            m_output.AssertReceived(expected);
        }
    }
}