namespace ArcticFox.PolyType.FormEncoded.Converters
{
    public class FormEnumConverter<TEnum, TUnderlying> : FormConverter<TEnum>
    {
        public required FormConverter<TUnderlying> m_underlying;
        
        public override TEnum? Read(ref FormDecoder decoder, ReadOnlySpan<char> value)
        {
            var underlying = m_underlying.Read(ref decoder, value)!;
            return (TEnum)(object)underlying;
        }

        public override void Write(ref FormEncoder encoder, TEnum? value)
        {
            m_underlying.Write(ref encoder, (TUnderlying)(object)value!);
        }
    }
}