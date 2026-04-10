namespace ArcticFox.PolyType.Sep.Converters
{
    public class SepStringValueConverter : SepValueConverter<string>
    {
        public override string Read(ReadOnlySpan<char> text)
        {
            return text.ToString();
        }
    }
}