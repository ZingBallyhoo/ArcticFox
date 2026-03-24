using System;
using System.Text;
using ArcticFox.Codec;

namespace ArcticFox.Tests.Impls
{
    public class TestEncodeCodecChain : IDisposable
    {
        private CodecChain<char> m_chain;
        private TestEncodeOutputCodec m_output;

        public TestEncodeCodecChain(Encoding? encoding = null)
        {
            encoding ??= Encoding.ASCII;
            
            m_output = new TestEncodeOutputCodec();
            
            m_chain = new CodecChain<char>();
            m_chain.AddCodec(new ZeroDelimitedEncodeCodec());
            m_chain.AddCodec(new TextEncodeCodec(encoding));
            m_chain.AddCodec(m_output);
        }

        public void DataInput(string str)
        {
            m_chain.Input2(str.AsSpan());
        }

        public void Dispose()
        {
            m_chain.Dispose();
        }

        public void AssertOutput(params string[] expected)
        {
            m_output.AssertOutput(expected);
        }
    }
}