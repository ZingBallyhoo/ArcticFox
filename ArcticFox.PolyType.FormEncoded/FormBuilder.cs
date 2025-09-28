using ArcticFox.PolyType.FormEncoded.Converters;
using PolyType;
using PolyType.Abstractions;
using PolyType.Utilities;

namespace ArcticFox.PolyType.FormEncoded
{
    internal class FormBuilder(TypeGenerationContext generationContext) : TypeShapeVisitor, ITypeShapeFunc
    {
        private ITypeShapeFunc self => generationContext;

        private FormConverter<T> ReEnter<T>(ITypeShape<T> typeShape, bool isDictKey=false)
        {
            return (FormConverter<T>)self.Invoke(typeShape, isDictKey)!;
        }

        public object? Invoke<T>(ITypeShape<T> typeShape, object? state = null)
        {
            return typeShape.Accept(this, state);
        }
    
        public override object? VisitObject<T>(IObjectTypeShape<T> type, object? state)
        {
            if (type.Type == typeof(string))
            {
                if (state is true) // isDictKey
                {
                    return new FormStringKeyConverter();
                }
                
                return new FormStringValueConverter();
            }
            
            if (type.Type == typeof(sbyte)) return new SimpleFormConverter<sbyte>();
            if (type.Type == typeof(byte)) return new SimpleFormConverter<byte>();
            if (type.Type == typeof(short)) return new SimpleFormConverter<short>();
            if (type.Type == typeof(ushort)) return new SimpleFormConverter<ushort>();
            if (type.Type == typeof(int)) return new SimpleFormConverter<int>();
            if (type.Type == typeof(uint)) return new SimpleFormConverter<uint>();
            if (type.Type == typeof(long)) return new SimpleFormConverter<long>();
            if (type.Type == typeof(ulong)) return new SimpleFormConverter<ulong>();
            if (type.Type == typeof(float)) return new SimpleFormConverter<float>();
            if (type.Type == typeof(double)) return new SimpleFormConverter<double>();
            
            if (type.Type == typeof(bool)) return new FormBoolConverter();
                
            return type.Constructor!.Accept(this);
        }
        
        public override object? VisitProperty<TDeclaringType, TPropertyType>(IPropertyShape<TDeclaringType, TPropertyType> propertyShape, object? state = null)
        {
            var propertyConverter = ReEnter(propertyShape.PropertyType);
            return new FormPropertyConverter<TDeclaringType, TPropertyType>(propertyShape, propertyConverter);
        }

        public override object? VisitConstructor<TDeclaringType, TArgumentState>(IConstructorShape<TDeclaringType, TArgumentState> constructorShape, object? state = null)
        {
            var properties = constructorShape.DeclaringType.Properties
                .Select(prop => (FormPropertyConverter<TDeclaringType>)prop.Accept(this)!)
                .ToArray();
            
            return new FormObjectConverter<TDeclaringType>(constructorShape.GetDefaultConstructor(), properties);
        }

        public override object? VisitEnum<TEnum, TUnderlying>(IEnumTypeShape<TEnum, TUnderlying> enumShape, object? state = null)
        {
            return new FormEnumConverter<TEnum, TUnderlying>
            {
                m_underlying = ReEnter(enumShape.UnderlyingType)
            };
        }

        public override object? VisitEnumerable<TEnumerable, TElement>(IEnumerableTypeShape<TEnumerable, TElement> enumerableShape, object? state = null)
        {
            var elementConverter = ReEnter(enumerableShape.ElementType);
            return enumerableShape.ConstructionStrategy switch
            {
                CollectionConstructionStrategy.Mutable => new FormMutableEnumerableConverter<TEnumerable, TElement>(elementConverter, enumerableShape),
                CollectionConstructionStrategy.Parameterized => new FormImmutableEnumerableConverter<TEnumerable, TElement>(elementConverter, enumerableShape),
                _ => new FormEnumerableConverter<TEnumerable, TElement>(elementConverter, enumerableShape)
            };
        }

        public override object? VisitDictionary<TDictionary, TKey, TValue>(IDictionaryTypeShape<TDictionary, TKey, TValue> dictionaryShape, object? state = null)
        {
            var keyConverter = ReEnter(dictionaryShape.KeyType, true);
            var valueConverter = ReEnter(dictionaryShape.ValueType);
            var getDictionary = dictionaryShape.GetGetDictionary();
            
            return new FormDictConverter<TDictionary, TKey, TValue>(keyConverter, valueConverter, getDictionary);
        }
    }
}