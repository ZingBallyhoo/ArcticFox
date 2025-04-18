namespace ArcticFox.PolyType.Amf
{
    public abstract class AmfConverter
    {
        internal AmfConverter() { }
        
        public abstract void WriteAsObject(ref AmfEncoder encoder, object? value);
        public abstract object? ReadAsObject(ref AmfDecoder decoder);
    }
    
    public abstract class AmfConverter<T> : AmfConverter
    {
        public abstract void Write(ref AmfEncoder encoder, T? value);
        public abstract T? Read(ref AmfDecoder decoder);

        public override void WriteAsObject(ref AmfEncoder encoder, object? value)
        {
            Write(ref encoder, (T?)value);
        }

        public override object? ReadAsObject(ref AmfDecoder decoder)
        {
            return Read(ref decoder);
        }
    }
}