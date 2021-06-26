using System.Buffers.Binary;

namespace ArcticFox.Codec.Binary
{
    public ref partial struct BitWriter
    {
        public void WriteDoubleBigEndian(double value)
        {
            FlushBit();
            BinaryPrimitives.WriteDoubleBigEndian(m_output.Slice(m_dataOffset, sizeof(double)), value);
            m_dataOffset += sizeof(double);
        }

        public void WriteDoubleLittleEndian(double value)
        {
            FlushBit();
            BinaryPrimitives.WriteDoubleLittleEndian(m_output.Slice(m_dataOffset, sizeof(double)), value);
            m_dataOffset += sizeof(double);
        }

        public void WriteInt16BigEndian(short value)
        {
            FlushBit();
            BinaryPrimitives.WriteInt16BigEndian(m_output.Slice(m_dataOffset, sizeof(short)), value);
            m_dataOffset += sizeof(short);
        }

        public void WriteInt16LittleEndian(short value)
        {
            FlushBit();
            BinaryPrimitives.WriteInt16LittleEndian(m_output.Slice(m_dataOffset, sizeof(short)), value);
            m_dataOffset += sizeof(short);
        }

        public void WriteInt32BigEndian(int value)
        {
            FlushBit();
            BinaryPrimitives.WriteInt32BigEndian(m_output.Slice(m_dataOffset, sizeof(int)), value);
            m_dataOffset += sizeof(int);
        }

        public void WriteInt32LittleEndian(int value)
        {
            FlushBit();
            BinaryPrimitives.WriteInt32LittleEndian(m_output.Slice(m_dataOffset, sizeof(int)), value);
            m_dataOffset += sizeof(int);
        }

        public void WriteInt64BigEndian(long value)
        {
            FlushBit();
            BinaryPrimitives.WriteInt64BigEndian(m_output.Slice(m_dataOffset, sizeof(long)), value);
            m_dataOffset += sizeof(long);
        }

        public void WriteInt64LittleEndian(long value)
        {
            FlushBit();
            BinaryPrimitives.WriteInt64LittleEndian(m_output.Slice(m_dataOffset, sizeof(long)), value);
            m_dataOffset += sizeof(long);
        }

        public void WriteSingleBigEndian(float value)
        {
            FlushBit();
            BinaryPrimitives.WriteSingleBigEndian(m_output.Slice(m_dataOffset, sizeof(float)), value);
            m_dataOffset += sizeof(float);
        }

        public void WriteSingleLittleEndian(float value)
        {
            FlushBit();
            BinaryPrimitives.WriteSingleLittleEndian(m_output.Slice(m_dataOffset, sizeof(float)), value);
            m_dataOffset += sizeof(float);
        }

        public void WriteUInt16BigEndian(ushort value)
        {
            FlushBit();
            BinaryPrimitives.WriteUInt16BigEndian(m_output.Slice(m_dataOffset, sizeof(ushort)), value);
            m_dataOffset += sizeof(ushort);
        }

        public void WriteUInt16LittleEndian(ushort value)
        {
            FlushBit();
            BinaryPrimitives.WriteUInt16LittleEndian(m_output.Slice(m_dataOffset, sizeof(ushort)), value);
            m_dataOffset += sizeof(ushort);
        }

        public void WriteUInt32BigEndian(uint value)
        {
            FlushBit();
            BinaryPrimitives.WriteUInt32BigEndian(m_output.Slice(m_dataOffset, sizeof(uint)), value);
            m_dataOffset += sizeof(uint);
        }

        public void WriteUInt32LittleEndian(uint value)
        {
            FlushBit();
            BinaryPrimitives.WriteUInt32LittleEndian(m_output.Slice(m_dataOffset, sizeof(uint)), value);
            m_dataOffset += sizeof(uint);
        }

        public void WriteUInt64BigEndian(ulong value)
        {
            FlushBit();
            BinaryPrimitives.WriteUInt64BigEndian(m_output.Slice(m_dataOffset, sizeof(ulong)), value);
            m_dataOffset += sizeof(ulong);
        }

        public void WriteUInt64LittleEndian(ulong value)
        {
            FlushBit();
            BinaryPrimitives.WriteUInt64LittleEndian(m_output.Slice(m_dataOffset, sizeof(ulong)), value);
            m_dataOffset += sizeof(ulong);
        }
    }
}