using System;

namespace ArcticFox.Codec.Binary
{
    public partial interface IBitWriter
    {
        void WriteBit(bool bit);
        void WriteByte(byte value);
        void WriteSByte(sbyte value);
        void WriteBits<T>(T obj, uint bitCount) where T : unmanaged;
        //void WriteBits(ref BitReader buffer, uint bitCount); // todo: is this useful externally? i dont think so
        void FlushBit();
        Span<byte> GetSpanOfNextBytes(int size);
        void WriteBytes(ReadOnlySpan<byte> data);
        void SeekByte(uint position);
        void SeekBit(uint position);
    }
}