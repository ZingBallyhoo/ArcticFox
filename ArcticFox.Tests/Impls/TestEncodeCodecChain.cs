using System;
using System.Text;
using ArcticFox.Codec;

namespace ArcticFox.Tests.Impls
{
    public class TestEncodeCodecChain : IDisposable
    {
        private readonly CodecChain<char> m_chain;
        private readonly TestEncodeOutputCodec m_output;

        public TestEncodeCodecChain(Encoding? encoding = null)
        {
            encoding ??= Encoding.ASCII;
            
            m_output = new TestEncodeOutputCodec();
            
            m_chain = new ZeroDelimitedEncodeCodec()
                .ChainTo(new TextEncodeCodec(encoding))
                .ChainTo(new TextDecodeCodec(encoding).ChainTo(new TextEncodeCodec(encoding))) // validate sub-chain
                .ChainTo(m_output);
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