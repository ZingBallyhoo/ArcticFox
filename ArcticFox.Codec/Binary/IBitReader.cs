using System;

namespace ArcticFox.Codec.Binary
{
    public partial interface IBitReader
    {
        byte ReadByte();
        sbyte ReadSByte();
        void ReadBytesTo(Span<byte> output, int count);
        void ReadBytesTo(Span<byte> output);
        bool ReadBit();
        T ReadBits<T>(uint bitCount) where T : unmanaged;
        void ReadBits(Span<byte> span, uint bitCount);
        void SkipBytes(int count);
        void SeekByte(uint position);
        void SeekBit(uint position);
        
    }
}