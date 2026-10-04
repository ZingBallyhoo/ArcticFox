using System.Text;
using ArcticFox.Codec.Binary;
using ArcticFox.PolyType.Amf.Zero;

namespace ArcticFox.PolyType.Amf
{
    public ref struct AmfDecoder
    {
        public BitReader m_reader;
        private readonly AmfOptions m_options;
        private int m_depth;
        
        public AmfOptions GetOptions() => m_options;
        
        public AmfDecoder(BitReader reader, AmfOptions options)
        {
            m_reader = reader;
            m_options = options;
        }
        
        public byte ReadByte()
        {
            return m_reader.ReadByte();
        }
        
        public Amf0TypeMarker ReadMarker() => (Amf0TypeMarker)ReadByte();
        
        public bool ReadBool()
        {
            return m_reader.ReadByte() != 0;
        }
        
        public ushort ReadUInt16()
        {
            return m_reader.ReadUInt16BigEndian();
        }
        
        public uint ReadUInt32()
        {
            return m_reader.ReadUInt32BigEndian();
        }

        public double ReadDouble()
        {
            return m_reader.ReadDoubleBigEndian();
        }
        
        public string ReadUtf8()
        {
            var length = ReadUInt16();
            var span = m_reader.ReadBytes(length);
            return Encoding.UTF8.GetString(span);
        }

        public int GetRemainingBytes()
        {
            return m_reader.m_dataLength - m_reader.m_dataOffset;
        }

        public void IncrementDepth()
        {
            m_depth++;
            if (m_depth > m_options.m_maxDepth)
            {
                throw new InvalidDataException("Recursion limit reached");
            }
        }

        public void DecrementDepth()
        {
            m_depth--;
        }
    }
}