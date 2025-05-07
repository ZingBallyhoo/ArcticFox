namespace ArcticFox.PolyType.FormEncoded.Converters
{
    public class FormStringValueConverter : FormConverter<string>
    {
        public override string? Read(ref FormDecoder decoder, ReadOnlySpan<char> value)
        {
            return value.ToString();
        }
        
        public override void Write(ref FormEncoder encoder, string? value)
        {
            encoder.WriteEncodedValue(value);
        }
    }
}