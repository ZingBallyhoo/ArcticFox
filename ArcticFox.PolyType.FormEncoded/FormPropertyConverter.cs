using PolyType.Abstractions;

namespace ArcticFox.PolyType.FormEncoded
{
    public abstract class FormPropertyConverter<TDeclaringType>(string name)
    {
        public string Name { get; } = name;
        public abstract bool HasGetter { get; }
        public abstract bool HasSetter { get; }
        
        public abstract void Read(ref FormDecoder decoder, ReadOnlySpan<char> value, ref TDeclaringType declaringType);
        public abstract void Write(ref FormEncoder encoder, ref TDeclaringType declaringType);
    }
    
    public class FormPropertyConverter<TDeclaringType, TPropertyType> : FormPropertyConverter<TDeclaringType>
    {
        private readonly FormConverter<TPropertyType> m_propertyConverter;
        private readonly Getter<TDeclaringType, TPropertyType>? _getter;
        private readonly Setter<TDeclaringType, TPropertyType>? _setter;
        
        public FormPropertyConverter(IPropertyShape<TDeclaringType, TPropertyType> property, FormConverter<TPropertyType> propertyConverter)
            : base(property.Name)
        {
            m_propertyConverter = propertyConverter;

            if (property.HasGetter)
            {
                _getter = property.GetGetter();
            }

            if (property.HasSetter) 
            { 
                _setter = property.GetSetter();
            }
        }
        
        public override bool HasGetter => _getter != null;
        public override bool HasSetter => _setter != null;

        public override void Read(ref FormDecoder decoder, ReadOnlySpan<char> value, ref TDeclaringType declaringType)
        {
            var result = m_propertyConverter.Read(ref decoder, value);
            _setter!(ref declaringType, result!);
        }

        public override void Write(ref FormEncoder encoder, ref TDeclaringType declaringType)
        {
            var value = _getter!(ref declaringType);
            m_propertyConverter.Write(ref encoder, value);
        }
    }
}