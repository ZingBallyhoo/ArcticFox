using System.Text;
using ArcticFox.Codec.Binary;
using ArcticFox.PolyType.Amf.Zero;

namespace ArcticFox.PolyType.Amf
{
    public ref struct AmfEncoder
    {
        public GrowingBitWriter m_writer;
        private readonly AmfOptions m_options;
        private int m_depth;
        
        public AmfEncoder(AmfOptions options)
        {
            m_writer = new GrowingBitWriter();
            m_options = options;
        }
        
        public void PutMarker(Amf0TypeMarker marker)
        {
            m_writer.WriteByte((byte)marker);
        }
        
        public void PutUInt8(byte value)
        {
            m_writer.WriteByte(value);
        }
        
        public void PutUInt16(ushort value)
        {
            m_writer.WriteUInt16BigEndian(value);
        }

        public void PutUtf8(string value)
        {
            var encoded = Encoding.UTF8.GetBytes(value);
            PutUInt16(checked((ushort)encoded.Length));
            m_writer.WriteBytes(encoded);
        }

        public void PutInt32(int value)
        {
            m_writer.WriteInt32BigEndian(value);
        }

        public void PutDouble(double value)
        {
            m_writer.WriteDoubleBigEndian(value);
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
        
        public void Dispose()
        {
            m_writer.Dispose();
        }
    }
}