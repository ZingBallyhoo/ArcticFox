namespace ArcticFox.PolyType.Sep.Converters
{
    public class SepSimpleValueConverter<T> : SepValueConverter<T> where T : ISpanParsable<T> 
    {
        public override T Read(ReadOnlySpan<char> text)
        {
            return T.Parse(text, null);
        }
    }
}