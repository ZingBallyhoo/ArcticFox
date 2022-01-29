using System;
using System.Runtime.InteropServices;

namespace ArcticFox.Codec.Binary
{
    public unsafe ref partial struct ZeroTruncatedBitReader
    {
        public double ReadDoubleBigEndian()
        {
            var remainingBytes = m_remainingBytes;
            if (remainingBytes == 0) return 0;
            if (remainingBytes >= sizeof(double))
            {
                return m_reader.ReadDoubleBigEndian();
            }

            double tBuffer = default;
            var buffer = new Span<byte>((byte*)&tBuffer, sizeof(double));
            ReadBytesTo(buffer, sizeof(double));

            var reader = new BitReader(buffer);
            return reader.ReadDoubleBigEndian();
        }

        public double ReadDoubleLittleEndian()
        {
            var remainingBytes = m_remainingBytes;
            if (remainingBytes == 0) return 0;
            if (remainingBytes >= sizeof(double))
            {
                return m_reader.ReadDoubleLittleEndian();
            }

            double tBuffer = default;
            var buffer = new Span<byte>((byte*)&tBuffer, sizeof(double));
            ReadBytesTo(buffer, sizeof(double));

            var reader = new BitReader(buffer);
            return reader.ReadDoubleLittleEndian();
        }

        public short ReadInt16BigEndian()
        {
            var remainingBytes = m_remainingBytes;
            if (remainingBytes == 0) return 0;
            if (remainingBytes >= sizeof(short))
            {
                return m_reader.ReadInt16BigEndian();
            }

            short tBuffer = default;
            var buffer = new Span<byte>((byte*)&tBuffer, sizeof(short));
            ReadBytesTo(buffer, sizeof(short));

            var reader = new BitReader(buffer);
            return reader.ReadInt16BigEndian();
        }

        public short ReadInt16LittleEndian()
        {
            var remainingBytes = m_remainingBytes;
            if (remainingBytes == 0) return 0;
            if (remainingBytes >= sizeof(short))
            {
                return m_reader.ReadInt16LittleEndian();
            }

            short tBuffer = default;
            var buffer = new Span<byte>((byte*)&tBuffer, sizeof(short));
            ReadBytesTo(buffer, sizeof(short));

            var reader = new BitReader(buffer);
            return reader.ReadInt16LittleEndian();
        }

        public int ReadInt32BigEndian()
        {
            var remainingBytes = m_remainingBytes;
            if (remainingBytes == 0) return 0;
            if (remainingBytes >= sizeof(int))
            {
                return m_reader.ReadInt32BigEndian();
            }

            int tBuffer = default;
            var buffer = new Span<byte>((byte*)&tBuffer, sizeof(int));
            ReadBytesTo(buffer, sizeof(int));

            var reader = new BitReader(buffer);
            return reader.ReadInt32BigEndian();
        }

        public int ReadInt32LittleEndian()
        {
            var remainingBytes = m_remainingBytes;
            if (remainingBytes == 0) return 0;
            if (remainingBytes >= sizeof(int))
            {
                return m_reader.ReadInt32LittleEndian();
            }

            int tBuffer = default;
            var buffer = new Span<byte>((byte*)&tBuffer, sizeof(int));
            ReadBytesTo(buffer, sizeof(int));

            var reader = new BitReader(buffer);
            return reader.ReadInt32LittleEndian();
        }

        public long ReadInt64BigEndian()
        {
            var remainingBytes = m_remainingBytes;
            if (remainingBytes == 0) return 0;
            if (remainingBytes >= sizeof(long))
            {
                return m_reader.ReadInt64BigEndian();
            }

            long tBuffer = default;
            var buffer = new Span<byte>((byte*)&tBuffer, sizeof(long));
            ReadBytesTo(buffer, sizeof(long));

            var reader = new BitReader(buffer);
            return reader.ReadInt64BigEndian();
        }

        public long ReadInt64LittleEndian()
        {
            var remainingBytes = m_remainingBytes;
            if (remainingBytes == 0) return 0;
            if (remainingBytes >= sizeof(long))
            {
                return m_reader.ReadInt64LittleEndian();
            }

            long tBuffer = default;
            var buffer = new Span<byte>((byte*)&tBuffer, sizeof(long));
            ReadBytesTo(buffer, sizeof(long));

            var reader = new BitReader(buffer);
            return reader.ReadInt64LittleEndian();
        }

        public float ReadSingleBigEndian()
        {
            var remainingBytes = m_remainingBytes;
            if (remainingBytes == 0) return 0;
            if (remainingBytes >= sizeof(float))
            {
                return m_reader.ReadSingleBigEndian();
            }

            float tBuffer = default;
            var buffer = new Span<byte>((byte*)&tBuffer, sizeof(float));
            ReadBytesTo(buffer, sizeof(float));

            var reader = new BitReader(buffer);
            return reader.ReadSingleBigEndian();
        }

        public float ReadSingleLittleEndian()
        {
            var remainingBytes = m_remainingBytes;
            if (remainingBytes == 0) return 0;
            if (remainingBytes >= sizeof(float))
            {
                return m_reader.ReadSingleLittleEndian();
            }

            float tBuffer = default;
            var buffer = new Span<byte>((byte*)&tBuffer, sizeof(float));
            ReadBytesTo(buffer, sizeof(float));

            var reader = new BitReader(buffer);
            return reader.ReadSingleLittleEndian();
        }

        public ushort ReadUInt16BigEndian()
        {
            var remainingBytes = m_remainingBytes;
            if (remainingBytes == 0) return 0;
            if (remainingBytes >= sizeof(ushort))
            {
                return m_reader.ReadUInt16BigEndian();
            }

            ushort tBuffer = default;
            var buffer = new Span<byte>((byte*)&tBuffer, sizeof(ushort));
            ReadBytesTo(buffer, sizeof(ushort));

            var reader = new BitReader(buffer);
            return reader.ReadUInt16BigEndian();
        }

        public ushort ReadUInt16LittleEndian()
        {
            var remainingBytes = m_remainingBytes;
            if (remainingBytes == 0) return 0;
            if (remainingBytes >= sizeof(ushort))
            {
                return m_reader.ReadUInt16LittleEndian();
            }

            ushort tBuffer = default;
            var buffer = new Span<byte>((byte*)&tBuffer, sizeof(ushort));
            ReadBytesTo(buffer, sizeof(ushort));

            var reader = new BitReader(buffer);
            return reader.ReadUInt16LittleEndian();
        }

        public uint ReadUInt32BigEndian()
        {
            var remainingBytes = m_remainingBytes;
            if (remainingBytes == 0) return 0;
            if (remainingBytes >= sizeof(uint))
            {
                return m_reader.ReadUInt32BigEndian();
            }

            uint tBuffer = default;
            var buffer = new Span<byte>((byte*)&tBuffer, sizeof(uint));
            ReadBytesTo(buffer, sizeof(uint));

            var reader = new BitReader(buffer);
            return reader.ReadUInt32BigEndian();
        }

        public uint ReadUInt32LittleEndian()
        {
            var remainingBytes = m_remainingBytes;
            if (remainingBytes == 0) return 0;
            if (remainingBytes >= sizeof(uint))
            {
                return m_reader.ReadUInt32LittleEndian();
            }

            uint tBuffer = default;
            var buffer = new Span<byte>((byte*)&tBuffer, sizeof(uint));
            ReadBytesTo(buffer, sizeof(uint));

            var reader = new BitReader(buffer);
            return reader.ReadUInt32LittleEndian();
        }

        public ulong ReadUInt64BigEndian()
        {
            var remainingBytes = m_remainingBytes;
            if (remainingBytes == 0) return 0;
            if (remainingBytes >= sizeof(ulong))
            {
                return m_reader.ReadUInt64BigEndian();
            }

            ulong tBuffer = default;
            var buffer = new Span<byte>((byte*)&tBuffer, sizeof(ulong));
            ReadBytesTo(buffer, sizeof(ulong));

            var reader = new BitReader(buffer);
            return reader.ReadUInt64BigEndian();
        }

        public ulong ReadUInt64LittleEndian()
        {
            var remainingBytes = m_remainingBytes;
            if (remainingBytes == 0) return 0;
            if (remainingBytes >= sizeof(ulong))
            {
                return m_reader.ReadUInt64LittleEndian();
            }

            ulong tBuffer = default;
            var buffer = new Span<byte>((byte*)&tBuffer, sizeof(ulong));
            ReadBytesTo(buffer, sizeof(ulong));

            var reader = new BitReader(buffer);
            return reader.ReadUInt64LittleEndian();
        }
    }
}