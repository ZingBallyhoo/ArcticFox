using ArcticFox.PolyType.Amf.Zero;
using PolyType.Abstractions;
using PolyType.Utilities;

namespace ArcticFox.PolyType.Amf
{
    internal sealed class Amf0Builder(TypeGenerationContext generationContext, AmfOptions options) : TypeShapeVisitor, ITypeShapeFunc
    {
        private ITypeShapeFunc self => generationContext;

        private AmfConverter<T> ReEnter<T>(ITypeShape<T> typeShape)
        {
            return (AmfConverter<T>)self.Invoke(typeShape)!;
        }

        public object? Invoke<T>(ITypeShape<T> typeShape, object? state = null)
        {
            return typeShape.Accept(this);
        }
    
        public override object? VisitObject<T>(IObjectTypeShape<T> type, object? state)
        {
            if (options.TryGetAmf0PrimitiveConverter(type.Type, out var primitiveConverter))
            {
                return primitiveConverter;
            }
            
            if (options.TryGetTypedObject(type.Type, out var moniker))
            {
                return type.Constructor!.Accept(this, state: moniker);
            }
            
            throw new NotImplementedException($"dont know how to process type: {type.Type}");
        }

        public override object? VisitProperty<TDeclaringType, TPropertyType>(IPropertyShape<TDeclaringType, TPropertyType> propertyShape, object? state = null)
        {
            var propertyConverter = ReEnter(propertyShape.PropertyType);
            return new Amf0PropertyConverter<TDeclaringType, TPropertyType>(propertyShape, propertyConverter);
        }

        public override object? VisitConstructor<TDeclaringType, TArgumentState>(IConstructorShape<TDeclaringType, TArgumentState> constructorShape, object? state = null)
        {
            var properties = constructorShape.DeclaringType.Properties
                .Select(prop => (Amf0PropertyConverter<TDeclaringType>)prop.Accept(this)!)
                .ToArray();
            
            var moniker = (string)state!;
            return new Amf0TypedObjectConverter<TDeclaringType>(moniker, constructorShape.GetDefaultConstructor(), properties);
        }

        public override object? VisitEnum<TEnum, TUnderlying>(IEnumTypeShape<TEnum, TUnderlying> enumShape, object? state = null)
        {
            return new Amf0EnumConverter<TEnum, TUnderlying>
            {
                m_underlying = ReEnter(enumShape.UnderlyingType)
            };
        }
        
        public override object? VisitEnumerable<TEnumerable, TElement>(IEnumerableTypeShape<TEnumerable, TElement> enumerableShape, object? state)
        {
            var elementConverter = ReEnter(enumerableShape.ElementType);
            return new Amf0EnumerableConverter<TEnumerable, TElement>(options, elementConverter, enumerableShape);
        }
    }
}