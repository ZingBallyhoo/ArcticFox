namespace ArcticFox.PolyType.Amf.Zero
{
    public class Amf0EnumConverter<TEnum, TUnderlying> : AmfConverter<TEnum>
    {
        public required AmfConverter<TUnderlying> m_underlying;

        public override void Write(ref AmfEncoder encoder, TEnum? value)
        {
            m_underlying.Write(ref encoder, (TUnderlying)(object)value!);
        }

        public override TEnum? Read(ref AmfDecoder decoder)
        {
            var underlying = m_underlying.Read(ref decoder)!;
            return (TEnum)(object)underlying;
        }
    }
}