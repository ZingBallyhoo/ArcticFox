using System.Dynamic;
using PolyType;

namespace ArcticFox.PolyType.Amf.Zero
{
    public class Amf0DynamicValueConverter(AmfOptions options, ITypeShapeProvider provider) : AmfConverter<object>
    {
        private AmfConverter ResolveConverter(Type type)
        {
            var converter = options.GetAmf0Converter(type, provider);
            if (converter is Amf0DynamicValueConverter or null)
            {
                throw new Exception($"unable to resolve converter for type: {type}");
            }
            
            return converter;
        }
        
        public override void Write(ref AmfEncoder encoder, object? value)
        {
            if (value == null)
            {
                encoder.PutMarker(Amf0TypeMarker.Null);
                return;
            }
            
            var converter = ResolveConverter(value.GetType());
            converter.WriteAsObject(ref encoder, value);
        }

        public override object? Read(ref AmfDecoder decoder)
        {
            var peekDecoder = decoder; // copy
            var peekedMarker = peekDecoder.ReadMarker();

            switch (peekedMarker)
            {
                case Amf0TypeMarker.String:
                case Amf0TypeMarker.LongString:
                {
                    return ResolveConverter(typeof(string)).ReadAsObject(ref decoder);
                }
                case Amf0TypeMarker.StrictArray:
                case Amf0TypeMarker.EcmaArray:
                {
                    return ResolveConverter(typeof(object[])).ReadAsObject(ref decoder);
                }
                case Amf0TypeMarker.Number:
                {
                    return ResolveConverter(typeof(double)).ReadAsObject(ref decoder);
                }
                case Amf0TypeMarker.Object:
                {
                    return ResolveConverter(typeof(ExpandoObject)).ReadAsObject(ref decoder);
                }
                case Amf0TypeMarker.TypedObject:
                {
                    var peekedName = peekDecoder.ReadUtf8();
                    if (!options.TryGetTypedObject(peekedName, out var type))
                    {
                        throw new Exception($"unknown typed object: {peekedName}");
                    }
                    return ResolveConverter(type).ReadAsObject(ref decoder);
                }
            }
            
            throw new Exception($"unknown marker: {peekedMarker}");
        }
    }
}