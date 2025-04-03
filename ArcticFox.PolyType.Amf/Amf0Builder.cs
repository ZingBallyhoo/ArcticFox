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
            
            var properties = type.Properties
                .Select(prop => (Amf0PropertyConverter<T>)prop.Accept(this)!)
                .ToArray();
            return new Amf0TypedObjectConverter<T>(properties);
        }

        public override object? VisitProperty<TDeclaringType, TPropertyType>(IPropertyShape<TDeclaringType, TPropertyType> propertyShape, object? state = null)
        {
            var propertyConverter = ReEnter(propertyShape.PropertyType);
            return new Amf0PropertyConverter<TDeclaringType, TPropertyType>(propertyShape, propertyConverter);
        }
        
        public override object? VisitEnumerable<TEnumerable, TElement>(IEnumerableTypeShape<TEnumerable, TElement> enumerableShape, object? state)
        {
            var elementConverter = ReEnter(enumerableShape.ElementType);
            return new Amf0EnumerableConverter<TEnumerable, TElement>(options, elementConverter, enumerableShape);
        }

        public override object? VisitDictionary<TDictionary, TKey, TValue>(IDictionaryTypeShape<TDictionary, TKey, TValue> dictionaryShape, object? state = null)
        {
            // (ExpandoObject only)
            return new Amf0AnonymousObjectConverter(options.GetAmf0Converter(typeof(object), generationContext.ParentCache!.Provider!));
        }
    }
}