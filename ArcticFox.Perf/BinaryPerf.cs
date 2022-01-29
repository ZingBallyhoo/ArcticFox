using ArcticFox.Codec.Binary;
using BenchmarkDotNet.Attributes;

namespace ArcticFox.Perf
{
    [SimpleJob]
    public class BinaryPerf
    {
        private byte[] m_bytes1;
        private const int c_count = 1000;
        
        [Params(0, 1)] public int m_offset;
        
        [GlobalSetup]
        public void GlobalSetup()
        {
            using var growingWriter = new GrowingBitWriter();
            for (var i = 0; i < m_offset; i++)
            {
                growingWriter.WriteBit(false);
            }

            for (var i = 0; i < c_count; i++)
            {
                growingWriter.WriteBits<uint>(999, 32);
            }

            m_bytes1 = growingWriter.GetData().ToArray();
        }

        [Benchmark]
        public void ReadAsSingles()
        {
            var reader = new BitReader(m_bytes1);
            for (var i = 0; i < m_offset; i++)
            {
                reader.ReadBit();
            }
            
            for (var i = 0; i < c_count; i++)
            {
                reader.ReadBits<uint>(32);
            }
        }
        
        [Benchmark]
        public void ReadWholeThing()
        {
            var reader = new BitReader(m_bytes1);
            for (var i = 0; i < m_offset; i++)
            {
                reader.ReadBit();
            }
            reader.ReadBits<HugeStruct>(c_count * 32);
        }

        private unsafe struct HugeStruct
        {
            public fixed int m_buf[c_count];
        }
    }
}