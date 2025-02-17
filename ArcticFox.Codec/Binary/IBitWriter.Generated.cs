using System;

namespace ArcticFox.Codec.Binary
{
    public partial interface IBitWriter
    {
        void WriteDoubleBigEndian(double value);
        void WriteDoubleLittleEndian(double value);
        void WriteInt16BigEndian(short value);
        void WriteInt16LittleEndian(short value);
        void WriteInt32BigEndian(int value);
        void WriteInt32LittleEndian(int value);
        void WriteInt64BigEndian(long value);
        void WriteInt64LittleEndian(long value);
        void WriteSingleBigEndian(float value);
        void WriteSingleLittleEndian(float value);
        void WriteUInt16BigEndian(ushort value);
        void WriteUInt16LittleEndian(ushort value);
        void WriteUInt32BigEndian(uint value);
        void WriteUInt32LittleEndian(uint value);
        void WriteUInt64BigEndian(ulong value);
        void WriteUInt64LittleEndian(ulong value);

    }
}