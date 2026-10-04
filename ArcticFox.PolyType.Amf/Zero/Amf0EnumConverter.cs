namespace ArcticFox.PolyType.Amf.Zero
{
    public class Amf0EnumConverter<TEnum, TUnderlying> : AmfConverter<TEnum>
    {
        public required AmfConverter<TUnderlying> m_underlying;

        protected override void Write(ref AmfEncoder encoder, TEnum? value)
        {
            m_underlying.WriteChecked(ref encoder, (TUnderlying)(object)value!);
        }

        protected override TEnum? Read(ref AmfDecoder decoder)
        {
            var underlying = m_underlying.ReadChecked(ref decoder)!;
            return (TEnum)(object)underlying;
        }
    }
}