namespace ArcticFox.PolyType.FormEncoded.Converters
{
    public class FormBoolConverter : FormConverter<bool>
    {
        public override bool Read(ref FormDecoder decoder, ReadOnlySpan<char> value)
        {
            return value switch
            {
                "true" => true,
                "false" => false,
                _ => throw new InvalidDataException($"unknown bool token: \"{value}\"")
            };
        }
        
        public override void Write(ref FormEncoder encoder, bool value)
        {
            throw new NotImplementedException();
        }
    }
}