using System.Globalization;

namespace ArcticFox.PolyType.FormEncoded.Converters
{
    public class SimpleFormConverter<T> : FormConverter<T> where T : ISpanFormattable, ISpanParsable<T>
    {
        public override T? Read(ref FormDecoder decoder, ReadOnlySpan<char> value)
        {
            return T.Parse(decoder.DecodeValue(value), CultureInfo.InvariantCulture);
        }
        
        public override void Write(ref FormEncoder encoder, T? value)
        {
            // todo: string alloc...
            // we need to encode the `-` on `-45`
            encoder.WriteEncodedValue($"{value}");
        }
    }
}