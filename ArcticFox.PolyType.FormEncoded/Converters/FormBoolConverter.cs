namespace ArcticFox.PolyType.FormEncoded.Converters
{
    public class FormBoolConverter : FormConverter<bool>
    {
        public override bool Read(ref FormDecoder decoder, ReadOnlySpan<char> value)
        {
            return value switch
            {
                "0" => false,
                "1" => true,
                "true" => true,
                "false" => false,
                _ => throw new InvalidDataException($"unknown bool token: \"{value}\"")
            };
        }
        
        public override void Write(ref FormEncoder encoder, bool value)
        {
            encoder.m_writer.Append(value ? '1' : '0');
        }
    }
}