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
        protected abstract void Write(ref AmfEncoder encoder, T? value);
        protected abstract T? Read(ref AmfDecoder decoder);
        
        public void WriteChecked(ref AmfEncoder encoder, T? value)
        {
            encoder.IncrementDepth(); 
            Write(ref encoder, value);
            encoder.DecrementDepth();
        }

        public T? ReadChecked(ref AmfDecoder decoder)
        {
            decoder.IncrementDepth();
            var value = Read(ref decoder);
            decoder.DecrementDepth();
            return value;
        }

        public override void WriteAsObject(ref AmfEncoder encoder, object? value)
        {
            WriteChecked(ref encoder, (T?)value);
        }

        public override object? ReadAsObject(ref AmfDecoder decoder)
        {
            return ReadChecked(ref decoder);
        }
    }
}