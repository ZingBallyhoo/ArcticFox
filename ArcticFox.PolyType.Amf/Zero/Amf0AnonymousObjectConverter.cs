using System.Dynamic;

namespace ArcticFox.PolyType.Amf.Zero
{
    public class Amf0AnonymousObjectConverter(AmfConverter propertyConverter) : AmfConverter<ExpandoObject>
    {
        protected override void Write(ref AmfEncoder encoder, ExpandoObject? value)
        {
            throw new NotImplementedException();
        }

        protected override ExpandoObject? Read(ref AmfDecoder decoder)
        {
            var marker = decoder.ReadMarker();
            if (marker != Amf0TypeMarker.Object)
            {
                throw new NotImplementedException($"unknown anonymous object marker: {marker}");
            }
            
            var expando = new ExpandoObject();
            IDictionary<string, object?> dict = expando;

            var propertyLimit = decoder.GetOptions().m_maxAnonymousProperties;
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
                
                if (dict.Count >= propertyLimit)
                {
                    throw new InvalidDataException($"number of anonymous properties over configured limit. (at least){dict.Count} > {propertyLimit}");
                }
                
                dict.Add(propertyName, propertyConverter.ReadAsObject(ref decoder));
            }
            
            return expando;
        }
    }
}