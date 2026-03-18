using PolyType;

namespace ArcticFox.PolyType.FormEncoded.Converters
{
    public class FormSurrogateConverter<T, TSurrogate>(
        IMarshaler<T, TSurrogate> marshaler,
        FormConverter<TSurrogate> surrogateConverter) : FormConverter<T>
    {
        public override T? Read(ref FormDecoder decoder, ReadOnlySpan<char> value)
        {
            return marshaler.Unmarshal(surrogateConverter.Read(ref decoder, value));
        }

        public override void Write(ref FormEncoder encoder, T? value)
        {
            surrogateConverter.Write(ref encoder, marshaler.Marshal(value)!);
        }
    }
}