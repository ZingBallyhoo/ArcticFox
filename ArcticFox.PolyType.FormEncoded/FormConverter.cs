namespace ArcticFox.PolyType.FormEncoded
{
    public abstract class FormConverter
    {
        internal FormConverter() { }
    }
    
    public abstract class FormConverter<T> : FormConverter
    {
        public abstract T? Read(ref FormDecoder decoder, ReadOnlySpan<char> value);
        public abstract void Write(ref FormEncoder encoder, T? value);
    }
}