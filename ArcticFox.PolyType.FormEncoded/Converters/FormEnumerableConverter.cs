using System.Runtime.InteropServices;
using PolyType.Abstractions;

namespace ArcticFox.PolyType.FormEncoded.Converters
{
    public class FormEnumerableConverter<TEnumerable, TElement>(
        FormConverter<TElement> elementConverter, 
        IEnumerableTypeShape<TEnumerable, TElement> typeShape) : FormConverter<TEnumerable>
    {
        private readonly Func<TEnumerable, IEnumerable<TElement>> m_getEnumerable = typeShape.GetGetEnumerable();
        private protected readonly FormConverter<TElement> m_elementConverter = elementConverter;
        
        public override TEnumerable? Read(ref FormDecoder decoder, ReadOnlySpan<char> value)
        {
            throw new NotSupportedException($"Deserialization not supported for type {typeof(TEnumerable)}.");
        }
        
        public override void Write(ref FormEncoder encoder, TEnumerable? value)
        {
            if (value == null) return;
            
            var enumerable = m_getEnumerable(value);
            
            var first = true;
            foreach (var element in enumerable)
            {
                if (first) first = false;
                else encoder.m_writer.Append(encoder.m_options.m_nextValueDelimiter);
                m_elementConverter.Write(ref encoder, element);
            }
        }
    }
    
    public class FormMutableEnumerableConverter<TEnumerable, TElement>(
        FormConverter<TElement> elementConverter,
        IEnumerableTypeShape<TEnumerable, TElement> typeShape)
        : FormEnumerableConverter<TEnumerable, TElement>(elementConverter, typeShape)
    {
        private readonly Func<TEnumerable> m_createObject = typeShape.GetDefaultConstructor();
        private readonly Setter<TEnumerable, TElement> m_addDelegate = typeShape.GetAddElement();
        
        public override TEnumerable? Read(ref FormDecoder decoder, ReadOnlySpan<char> value)
        {
            var result = m_createObject();
            
            foreach (var subRange in value.Split(decoder.m_options.m_nextValueDelimiter))
            {
                var rangeSpan = value[subRange];
                m_addDelegate(ref result, m_elementConverter.Read(ref decoder, rangeSpan)!);
            }
            
            return result;
        }
    }

    public class FormImmutableEnumerableConverter<TEnumerable, TElement>(
        FormConverter<TElement> elementConverter,
        IEnumerableTypeShape<TEnumerable, TElement> typeShape)
        : FormEnumerableConverter<TEnumerable, TElement>(elementConverter, typeShape)
    {
        public override TEnumerable? Read(ref FormDecoder decoder, ReadOnlySpan<char> value)
        {
            var list = new List<TElement>();
            
            foreach (var subRange in value.Split(decoder.m_options.m_nextValueDelimiter))
            {
                var rangeSpan = value[subRange];
                list.Add(m_elementConverter.Read(ref decoder, rangeSpan)!);
            }
            
            return typeShape.ConstructionStrategy switch
            {
                CollectionConstructionStrategy.Span => typeShape.GetSpanConstructor()(CollectionsMarshal.AsSpan(list)),
                CollectionConstructionStrategy.Enumerable => typeShape.GetEnumerableConstructor()(list),
                _ => throw new NotImplementedException($"can't construct enumerable: {typeShape}. {typeShape.ConstructionStrategy}")
            };
        }
    }
}