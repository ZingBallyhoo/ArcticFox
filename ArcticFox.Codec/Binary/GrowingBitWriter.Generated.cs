namespace ArcticFox.Codec.Binary
{
    public ref partial struct GrowingBitWriter
    {
        public void WriteDoubleBigEndian(double value)
        {
            if (m_effectiveByteOffset + sizeof(double) > m_memorySize)
            {
                Grow();
            }

            m_writer.WriteDoubleBigEndian(value);
        }

        public void WriteDoubleLittleEndian(double value)
        {
            if (m_effectiveByteOffset + sizeof(double) > m_memorySize)
            {
                Grow();
            }

            m_writer.WriteDoubleLittleEndian(value);
        }

        public void WriteInt16BigEndian(short value)
        {
            if (m_effectiveByteOffset + sizeof(short) > m_memorySize)
            {
                Grow();
            }

            m_writer.WriteInt16BigEndian(value);
        }

        public void WriteInt16LittleEndian(short value)
        {
            if (m_effectiveByteOffset + sizeof(short) > m_memorySize)
            {
                Grow();
            }

            m_writer.WriteInt16LittleEndian(value);
        }

        public void WriteInt32BigEndian(int value)
        {
            if (m_effectiveByteOffset + sizeof(int) > m_memorySize)
            {
                Grow();
            }

            m_writer.WriteInt32BigEndian(value);
        }

        public void WriteInt32LittleEndian(int value)
        {
            if (m_effectiveByteOffset + sizeof(int) > m_memorySize)
            {
                Grow();
            }

            m_writer.WriteInt32LittleEndian(value);
        }

        public void WriteInt64BigEndian(long value)
        {
            if (m_effectiveByteOffset + sizeof(long) > m_memorySize)
            {
                Grow();
            }

            m_writer.WriteInt64BigEndian(value);
        }

        public void WriteInt64LittleEndian(long value)
        {
            if (m_effectiveByteOffset + sizeof(long) > m_memorySize)
            {
                Grow();
            }

            m_writer.WriteInt64LittleEndian(value);
        }

        public void WriteSingleBigEndian(float value)
        {
            if (m_effectiveByteOffset + sizeof(float) > m_memorySize)
            {
                Grow();
            }

            m_writer.WriteSingleBigEndian(value);
        }

        public void WriteSingleLittleEndian(float value)
        {
            if (m_effectiveByteOffset + sizeof(float) > m_memorySize)
            {
                Grow();
            }

            m_writer.WriteSingleLittleEndian(value);
        }

        public void WriteUInt16BigEndian(ushort value)
        {
            if (m_effectiveByteOffset + sizeof(ushort) > m_memorySize)
            {
                Grow();
            }

            m_writer.WriteUInt16BigEndian(value);
        }

        public void WriteUInt16LittleEndian(ushort value)
        {
            if (m_effectiveByteOffset + sizeof(ushort) > m_memorySize)
            {
                Grow();
            }

            m_writer.WriteUInt16LittleEndian(value);
        }

        public void WriteUInt32BigEndian(uint value)
        {
            if (m_effectiveByteOffset + sizeof(uint) > m_memorySize)
            {
                Grow();
            }

            m_writer.WriteUInt32BigEndian(value);
        }

        public void WriteUInt32LittleEndian(uint value)
        {
            if (m_effectiveByteOffset + sizeof(uint) > m_memorySize)
            {
                Grow();
            }

            m_writer.WriteUInt32LittleEndian(value);
        }

        public void WriteUInt64BigEndian(ulong value)
        {
            if (m_effectiveByteOffset + sizeof(ulong) > m_memorySize)
            {
                Grow();
            }

            m_writer.WriteUInt64BigEndian(value);
        }

        public void WriteUInt64LittleEndian(ulong value)
        {
            if (m_effectiveByteOffset + sizeof(ulong) > m_memorySize)
            {
                Grow();
            }

            m_writer.WriteUInt64LittleEndian(value);
        }
    }
}