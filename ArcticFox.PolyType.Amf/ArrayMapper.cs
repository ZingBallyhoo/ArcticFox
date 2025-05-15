using PolyType;
using PolyType.Abstractions;
using PolyType.Utilities;

namespace ArcticFox.PolyType.Amf
{
    public static class ArrayMapper
    {
        private static readonly MultiProviderTypeCache s_cache = new MultiProviderTypeCache
        {
            ValueBuilderFactory = ctx => new Builder(ctx)
            // todo: delay
        };
        
        private sealed class Builder(TypeGenerationContext generationContext) : TypeShapeVisitor, ITypeShapeFunc
        {
            private ITypeShapeFunc self => generationContext;

            private Converter<T> ReEnter<T>(ITypeShape<T> typeShape)
            {
                return (Converter<T>)self.Invoke(typeShape)!;
            }

            public object? Invoke<T>(ITypeShape<T> typeShape, object? state = null)
            {
                return typeShape.Accept(this);
            }

            public override object? VisitObject<T>(IObjectTypeShape<T> type, object? state = null)
            {
                if (type.Type.IsAssignableTo(typeof(IConvertible)))
                {
                    return new BuiltInConverter<T>();
                }
                
                var ctorShape = (IConstructorShape<T, object>)type.Constructor!;
                var properties = type.Properties
                    .Select(prop => (PropertyConverter<T>)prop.Accept(this)!)
                    .ToArray();
                return new ObjectConverter<T>(ctorShape.GetDefaultConstructor(), properties);
            }

            public override object? VisitProperty<TDeclaringType, TPropertyType>(IPropertyShape<TDeclaringType, TPropertyType> propertyShape, object? state = null)
            {
                var propertyConverter = ReEnter(propertyShape.PropertyType);
                return new PropertyConverter<TDeclaringType, TPropertyType>(propertyShape, propertyConverter);
            }

            public override object? VisitEnum<TEnum, TUnderlying>(IEnumTypeShape<TEnum, TUnderlying> enumShape, object? state = null)
            {
                return new EnumConverter<TEnum, TUnderlying>
                {
                    m_underlying = ReEnter(enumShape.UnderlyingType)
                };
            }
        }
        
        private abstract class Converter<T>
        {
            public abstract T Read(object? value);
            public virtual object? Write(T value)
            {
                return value;
            }
        }
        
        private class BuiltInConverter<T> : Converter<T>
        {
            public override T Read(object? value)
            {
                return (T)Convert.ChangeType(value, typeof(T))!;
            }
        }
        
        private class EnumConverter<TEnum, TUnderlying> : Converter<TEnum>
        {
            public required Converter<TUnderlying> m_underlying;

            public override TEnum Read(object? value)
            {
                var underlying = m_underlying.Read(value)!;
                return (TEnum)(object)underlying;
            }
        }
        
        private abstract class PropertyConverter<TDeclaringType>
        {
            public abstract void Read(object? value, ref TDeclaringType declaringType);
            public abstract object? Write(ref TDeclaringType declaringType);
        }
        
        private class PropertyConverter<TDeclaringType, TPropertyType> : PropertyConverter<TDeclaringType>
        {
            private readonly Getter<TDeclaringType, TPropertyType>? m_getter;
            private readonly Setter<TDeclaringType, TPropertyType>? m_setter;
            private readonly Converter<TPropertyType> m_propertyConverter;
        
            public PropertyConverter(IPropertyShape<TDeclaringType, TPropertyType> property, Converter<TPropertyType> propertyConverter)
            {
                m_getter = property.GetGetter();
                m_setter = property.GetSetter();
                m_propertyConverter = propertyConverter;
            }
            
            public override void Read(object? value, ref TDeclaringType declaringType)
            {
                var convertedValue = m_propertyConverter.Read(value);
                m_setter!(ref declaringType, (TPropertyType)convertedValue!);
            }

            public override object? Write(ref TDeclaringType declaringType)
            {
                var rawValue = m_getter!(ref declaringType);
                var convertedValue = m_propertyConverter.Read(rawValue);
                return convertedValue;
            }
        }
        
        private class ObjectConverter<T>(Func<T> defaultConstructor, PropertyConverter<T>[] properties) : Converter<T>
        {
            public override T Read(object? value)
            {
                var array = (object?[])value!;
                if (properties.Length != array.Length)
                {
                    throw new InvalidDataException("array length doesnt match property count");
                }
                
                var inst = defaultConstructor();
                for (var i = 0; i < array.Length; i++)
                {
                    properties[i].Read(array[i], ref inst);
                }
                
                return inst;
            }

            public override object Write(T value)
            {
                var array = new object?[properties.Length];
                for (var i = 0; i < array.Length; i++)
                {
                    array[i] = properties[i].Write(ref value);
                }
                return array;
            }
        }
        
        public static T ToObject<T>(object? arrayRaw) where T : IShapeable<T>
        {
            var converter = (Converter<T>)s_cache.GetOrAdd(T.GetShape())!;
            return converter.Read(arrayRaw);
        }
        
        public static object? ToArray<T>(T obj) where T : IShapeable<T>
        {
            var converter = (Converter<T>)s_cache.GetOrAdd(T.GetShape())!;
            return converter.Write(obj);
        }
    }
}