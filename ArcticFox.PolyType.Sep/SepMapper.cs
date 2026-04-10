using ArcticFox.PolyType.Sep.Converters;
using nietras.SeparatedValues;
using PolyType;
using PolyType.Abstractions;
using PolyType.Utilities;

namespace ArcticFox.PolyType.Sep
{
    public class SepMapper
    {
        private static readonly MultiProviderTypeCache s_cache = new MultiProviderTypeCache
        {
            ValueBuilderFactory = ctx => new Builder(ctx)
            // todo: delay
        };
        
        private class Builder(TypeGenerationContext generationContext) : TypeShapeVisitor, ITypeShapeFunc
        {
            private ITypeShapeFunc self => generationContext;

            private SepConverter<T> ReEnter<T>(ITypeShape<T> typeShape, bool isDictKey=false)
            {
                return (SepConverter<T>)self.Invoke(typeShape, isDictKey)!;
            }

            public object? Invoke<T>(ITypeShape<T> typeShape, object? state = null)
            {
                return typeShape.Accept(this, state);
            }

            public override object? VisitObject<T>(IObjectTypeShape<T> objectShape, object? state = null)
            {
                // todo: assuming value-level
                if (objectShape.Type == typeof(string)) return new SepStringValueConverter();
                if (objectShape.Type == typeof(byte)) return new SepSimpleValueConverter<byte>();
                if (objectShape.Type == typeof(short)) return new SepSimpleValueConverter<short>();
                if (objectShape.Type == typeof(ushort)) return new SepSimpleValueConverter<ushort>();
                if (objectShape.Type == typeof(int)) return new SepSimpleValueConverter<int>();
                if (objectShape.Type == typeof(uint)) return new SepSimpleValueConverter<uint>();
                if (objectShape.Type == typeof(long)) return new SepSimpleValueConverter<long>();
                if (objectShape.Type == typeof(ulong)) return new SepSimpleValueConverter<ulong>();
                if (objectShape.Type == typeof(float)) return new SepSimpleValueConverter<float>();
                if (objectShape.Type == typeof(double)) return new SepSimpleValueConverter<double>();
                
                // todo: assuming top-level
                return objectShape.Constructor!.Accept(this);
            }

            public override object? VisitProperty<TDeclaringType, TPropertyType>(IPropertyShape<TDeclaringType, TPropertyType> propertyShape, object? state = null)
            {
                // todo: don't cache, we don't want to end up with a top-level object reader
                var propertyConverter = (SepValueConverter<TPropertyType>)propertyShape.PropertyType.Accept(this)!;
                return new SepPropertyConverter<TDeclaringType, TPropertyType>(propertyShape, propertyConverter);
            }
            
            public override object? VisitConstructor<TDeclaringType, TArgumentState>(IConstructorShape<TDeclaringType, TArgumentState> constructorShape, object? state = null)
            {
                var properties = constructorShape.DeclaringType.Properties
                    .Select(prop => (SepPropertyConverter<TDeclaringType>)prop.Accept(this)!)
                    .ToArray();
            
                return new SepObjectConverter<TDeclaringType>(constructorShape.GetDefaultConstructor(), properties);
            }
        }
        
        public static T FromRow<T>(SepReader reader) where T : IShapeable<T>
        {
            var converter = (SepConverter<T>)s_cache.GetOrAdd(TypeShapeResolver.Resolve<T>())!;
            return converter.Read(reader);
        }
    }
}