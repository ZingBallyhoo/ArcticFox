using System;

namespace ArcticFox.Codec.Binary
{
    public partial interface IBitReader
    {
        double ReadDoubleBigEndian();
        double ReadDoubleLittleEndian();
        short ReadInt16BigEndian();
        short ReadInt16LittleEndian();
        int ReadInt32BigEndian();
        int ReadInt32LittleEndian();
        long ReadInt64BigEndian();
        long ReadInt64LittleEndian();
        float ReadSingleBigEndian();
        float ReadSingleLittleEndian();
        ushort ReadUInt16BigEndian();
        ushort ReadUInt16LittleEndian();
        uint ReadUInt32BigEndian();
        uint ReadUInt32LittleEndian();
        ulong ReadUInt64BigEndian();
        ulong ReadUInt64LittleEndian();

    }
}