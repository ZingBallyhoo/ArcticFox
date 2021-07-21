using System;

namespace ArcticFox.RPC
{
    public interface IResponseDecoder
    {
        object DecodeResponse(ReadOnlySpan<byte> data, object? token);
    }
}