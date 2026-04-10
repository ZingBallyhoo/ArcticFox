namespace ArcticFox.PolyType.Sep
{
    public abstract class SepValueConverter<T>
    {
        public abstract T Read(ReadOnlySpan<char> text);
    }
}