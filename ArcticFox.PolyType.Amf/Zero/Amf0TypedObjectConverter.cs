namespace ArcticFox.PolyType.Amf.Zero
{
    public class Amf0TypedObjectConverter<T>(string moniker, Func<T> defaultConstructor, Amf0PropertyConverter<T>[] properties) : AmfConverter<T>
    {
        private readonly Amf0PropertyConverter<T>[] m_propertiesToWrite = properties.Where(prop => prop.HasGetter).ToArray();
        
        public override void Write(ref AmfEncoder encoder, T? value)
        {
            encoder.PutMarker(Amf0TypeMarker.TypedObject);
            encoder.PutUtf8(moniker);

            foreach (var propertyConverter in m_propertiesToWrite)
            {
                encoder.PutUtf8(propertyConverter.Name);
                propertyConverter.Write(ref encoder, ref value);
            }
            
            encoder.PutUtf8("");
            encoder.PutMarker(Amf0TypeMarker.ObjectEnd);
        }

        public override T? Read(ref AmfDecoder decoder)
        {
            var marker = decoder.ReadMarker();
            if (marker != Amf0TypeMarker.TypedObject)
            {
                throw new InvalidDataException($"expected TypedObject marker, got {marker}");
            }
            
            var readMoniker = decoder.ReadUtf8();
            if (!moniker.Equals(readMoniker))
            {
                throw new InvalidDataException($"wrong moniker for explicit typed object. found: {readMoniker}, expected: {moniker}");
            }
            
            var inst = defaultConstructor();
            while (true)
            {
                var propertyName = decoder.ReadUtf8();
                if (propertyName.Length == 0)
                {
                    var propertyValueMarker = decoder.ReadMarker();
                    if (propertyValueMarker != Amf0TypeMarker.ObjectEnd)
                    {
                        throw new InvalidDataException($"expected ObjectEnd, got {propertyValueMarker}");
                    }
                    break;
                }
                
                var property = m_propertiesToWrite.Single(x => x.Name == propertyName);
                property.Read(ref decoder, ref inst);
            }
            
            return inst;
        }
    }
}