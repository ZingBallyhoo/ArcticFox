using System;

namespace ArcticFox.RPC.Legacy
{
    public interface IResponseDecoder
    {
        object DecodeResponse(ReadOnlySpan<byte> data, object? token);
    }
}