using System;
using System.Text;

namespace ArcticFox.Tests
{
    public class TestEncodeCodecChain : IDisposable
    {
        private CodecChain m_chain;
        private TestEncodeOutputCodec m_output;

        public TestEncodeCodecChain(Encoding? encoding = null)
        {
            encoding ??= Encoding.ASCII;
            
            m_output = new TestEncodeOutputCodec();
            
            m_chain = new CodecChain();
            m_chain.AddCodec(new ZeroDelimitedEncodeCodec());
            m_chain.AddCodec(new TextEncodeCodec(encoding));
            m_chain.AddCodec(m_output);
        }

        public void DataInput(string str)
        {
            m_chain.Head<char>().Input(str);
        }

        public void Dispose()
        {
            m_chain.Dispose();
        }

        public void AssertOutput(params string[] expected)
        {
            m_output.AssertOutput(expected);
        }

        public void AssertAborted()
        {
            m_output.AssertAborted();
        }
    }
}