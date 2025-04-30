using ArcticFox.PolyType.FormEncoded.Converters;
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
            
            if (type.Type == typeof(int)) return new SimpleFormConverter<int>();
            if (type.Type == typeof(double)) return new SimpleFormConverter<double>();
            
            if (type.Type == typeof(bool)) return new FormBoolConverter();
                
            var ctorShape = (IConstructorShape<T, object>)type.Constructor!;
            var properties = type.Properties
                .Select(prop => (FormPropertyConverter<T>)prop.Accept(this)!)
                .ToArray();
            return new FormObjectConverter<T>(ctorShape.GetDefaultConstructor(), properties);
        }
        
        public override object? VisitProperty<TDeclaringType, TPropertyType>(IPropertyShape<TDeclaringType, TPropertyType> propertyShape, object? state = null)
        {
            var propertyConverter = ReEnter(propertyShape.PropertyType);
            return new FormPropertyConverter<TDeclaringType, TPropertyType>(propertyShape, propertyConverter);
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