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
        private readonly MutableCollectionConstructor<TElement, TEnumerable> m_mutableCtor = typeShape.GetDefaultConstructor();
        private readonly EnumerableAppender<TEnumerable, TElement> m_addDelegate = typeShape.GetAppender();
        
        public override TEnumerable Read(ref FormDecoder decoder, ReadOnlySpan<char> value)
        {
            // todo: initial capacity?
            var result = m_mutableCtor();
            
            foreach (var subRange in value.Split(decoder.m_options.m_nextValueDelimiter))
            {
                if (value.Length == 0) break; // whole input is empty string
                
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
        private readonly ParameterizedCollectionConstructor<TElement, TElement, TEnumerable> m_spanCtor = typeShape.GetParameterizedConstructor();
        
        public override TEnumerable Read(ref FormDecoder decoder, ReadOnlySpan<char> value)
        {
            // todo: initial capacity?
            // todo: or, reduce code duplication?
            var list = new List<TElement>();
            
            foreach (var subRange in value.Split(decoder.m_options.m_nextValueDelimiter))
            {
                if (value.Length == 0) break; // whole input is empty string
                
                var rangeSpan = value[subRange];
                list.Add(m_elementConverter.Read(ref decoder, rangeSpan)!);
            }
            
            return m_spanCtor(CollectionsMarshal.AsSpan(list));
        }
    }
}