using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Runtime.CompilerServices;
using ArcticFox.PolyType.Amf.Packet;
using ArcticFox.PolyType.Amf.Zero;
using PolyType;
using PolyType.Utilities;

namespace ArcticFox.PolyType.Amf
{
    public class AmfOptions
    {
        public static readonly AmfOptions Default = new AmfOptions();
        
        public int m_maxMessages = 10;
        public int m_maxHeaders = 10;
        public int m_maxArrayElements = 100;
        
        private readonly Dictionary<string, Type> m_typedObjectTypes = new Dictionary<string, Type>();
        private readonly Dictionary<Type, string> m_typedObjectMonikers = new Dictionary<Type, string>();
        
        private readonly MultiProviderTypeCache m_amf0ConverterCaches;
        private readonly ConditionalWeakTable<ITypeShapeProvider, AmfBuiltinConverters> m_builtinConverterCaches;
        
        private readonly Dictionary<Type, AmfConverter> m_amf0PrimitiveConverters;
        
        public AmfOptions()
        {
            m_amf0PrimitiveConverters = new Dictionary<Type, AmfConverter>
            {
                { typeof(string), new Amf0StringConverter() },
                { typeof(float), new Amf0NumberConverter<float>() },
                { typeof(double), new Amf0NumberConverter<double>() },
                { typeof(sbyte), new Amf0NumberConverter<sbyte>() },
                { typeof(byte), new Amf0NumberConverter<byte>() },
                { typeof(short), new Amf0NumberConverter<short>() },
                { typeof(ushort), new Amf0NumberConverter<ushort>() },
                { typeof(int), new Amf0NumberConverter<int>() },
                { typeof(uint), new Amf0NumberConverter<uint>() },
                { typeof(long), new Amf0NumberConverter<long>() },
                { typeof(ulong), new Amf0NumberConverter<ulong>() },
                { typeof(bool), new Amf0BoolConverter() }
            };
            
            m_amf0ConverterCaches = new MultiProviderTypeCache
            {
                ValueBuilderFactory = ctx => new Amf0Builder(ctx, this)
                // todo: delay
            };
            m_builtinConverterCaches = new ConditionalWeakTable<ITypeShapeProvider, AmfBuiltinConverters>();
        }
        
        public void AddTypedObject<T>() where T : IShapeable<T>
        {
            // todo: name override...?
            // would need to be shared with serializer impl
            m_typedObjectTypes[typeof(T).Name] = typeof(T);
            m_typedObjectMonikers[typeof(T)] = typeof(T).Name;
        }
        
        public bool TryGetTypedObject(Type type, [NotNullWhen(true)] out string? moniker)
        {
            return m_typedObjectMonikers.TryGetValue(type, out moniker);
        }
        
        public bool TryGetTypedObject(string moniker, [NotNullWhen(true)] out Type? type)
        {
            return m_typedObjectTypes.TryGetValue(moniker, out type);
        }
        
        public bool TryGetAmf0PrimitiveConverter(Type type, [NotNullWhen(true)] out AmfConverter? converter)
        {
            if (m_amf0PrimitiveConverters.TryGetValue(type, out converter))
            {
                return true;
            }
            
            if (type.IsPrimitive)
            {
                throw new Exception($"unimplemented amf0 primitive type: {type}");
            }
            return false;
        }
        
        public AmfConverter GetAmf0Converter(Type type, ITypeShapeProvider provider)
        {
            if (TryGetAmf0PrimitiveConverter(type, out var primitiveConverter))
            {
                return primitiveConverter;
            }
            
            // require because the packet is not in the shape provider...
            if (type == typeof(AmfPacket)) return GetBuiltinConverters(provider).m_packetConverter;
            if (type == typeof(ExpandoObject)) return GetBuiltinConverters(provider).m_anonymousObject;

            var typeShape = provider.GetTypeShapeOrThrow(type);
            var scopedCache = m_amf0ConverterCaches.GetScopedCache(typeShape);
            if (scopedCache.TryGetValue(type, out var existing))
            {
                return (AmfConverter)existing!;
            }
            
            return (AmfConverter)m_amf0ConverterCaches.GetOrAdd(typeShape)!;
        }

        private AmfBuiltinConverters GetBuiltinConverters(ITypeShapeProvider provider)
        {
            return m_builtinConverterCaches.GetValue(provider, key => new AmfBuiltinConverters(this, key));
        }

        private class AmfBuiltinConverters(AmfOptions options, ITypeShapeProvider provider)
        {
            private AmfConverter<object> m_objectConverter => (AmfConverter<object>)options.GetAmf0Converter(typeof(object), provider);
            public Amf0AnonymousObjectConverter m_anonymousObject => field ??= new Amf0AnonymousObjectConverter(m_objectConverter);
            
            private AmfHeaderConverter<object> m_headerConverter => field ??= new AmfHeaderConverter<object>(m_objectConverter);
            private AmfMessageConverter<object> m_messageConverter => field ??= new AmfMessageConverter<object>(m_objectConverter);
            public AmfPacketConverter m_packetConverter => field ??= new AmfPacketConverter(options, m_headerConverter, m_messageConverter);
        }
    }
}