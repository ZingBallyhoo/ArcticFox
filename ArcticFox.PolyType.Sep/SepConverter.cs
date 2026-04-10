using nietras.SeparatedValues;

namespace ArcticFox.PolyType.Sep
{
    public abstract class SepConverter<T>
    {
        public abstract T Read(SepReader reader);
    }
}