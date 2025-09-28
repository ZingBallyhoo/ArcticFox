using ArcticFox.Codec.Binary;
using PolyType;

namespace ArcticFox.PolyType.Amf
{
    public static class AmfPolyType
    {
        public static byte[] Serialize<T>(T value, AmfOptions options, ITypeShapeProvider provider)
        {
            var converter = (AmfConverter<T>)options.GetAmf0Converter(typeof(T), provider);
            
            var encoder = new AmfEncoder();
            try
            {
                converter.Write(ref encoder, value);
                return encoder.m_writer.GetData().ToArray();
            } finally
            {
                encoder.Dispose();
            }
        }
        public static byte[] Serialize<T, TProvider>(T value, AmfOptions options) where TProvider : IShapeable<T>
        {
            return Serialize(value, options, TProvider.GetTypeShape().Provider);
        }
        
        public static byte[] Serialize<T>(T value, AmfOptions options) where T : IShapeable<T>
        {
            return Serialize<T, T>(value, options);
        }
        
        public static T Deserialize<T>(scoped ReadOnlySpan<byte> data, AmfOptions options, ITypeShapeProvider provider)
        {
            var converter = (AmfConverter<T>)options.GetAmf0Converter(typeof(T), provider);
            
            var decoder = new AmfDecoder(new BitReader(data));
            return converter.Read(ref decoder)!;
        }
        
        public static T Deserialize<T, TProvider>(scoped ReadOnlySpan<byte> data, AmfOptions options) where TProvider : IShapeable<T>
        {
            return Deserialize<T>(data, options, TProvider.GetTypeShape().Provider);
        }
        
        public static T Deserialize<T>(scoped ReadOnlySpan<byte> data, AmfOptions options) where T : IShapeable<T>
        {
            return Deserialize<T, T>(data, options);
        }
    }
}