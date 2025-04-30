using System.Buffers;

namespace ArcticFox.PolyType.FormEncoded
{
    public static class UriHelper
    {
        // https://github.com/dotnet/runtime/blob/29acb2fd71a363053f1fbe67dc4bc06c555f7b94/src/libraries/System.Private.Uri/src/System/UriHelper.cs#L564
        
        // true for all ASCII letters and digits, as well as the RFC3986 reserved characters, unreserved characters, and hash
        public static readonly SearchValues<char> UnreservedReserved =
            SearchValues.Create("!#$&'()*+,-./0123456789:;=?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[]_abcdefghijklmnopqrstuvwxyz~");
    }
}