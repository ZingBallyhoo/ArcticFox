namespace ArcticFox.PolyType.FormEncoded.Converters
{
    public class FormStringKeyConverter : FormConverter<string>
    {
        public override string? Read(ref FormDecoder decoder, ReadOnlySpan<char> value)
        {
            return decoder.DecodeKey(value).ToString();
        }
        
        public override void Write(ref FormEncoder encoder, string? value)
        {
            encoder.WriteEncodedKey(value);
        }
    }
}