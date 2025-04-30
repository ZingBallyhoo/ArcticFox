using PolyType.Abstractions;

namespace ArcticFox.PolyType.FormEncoded.Converters
{
    public class FormEnumerableConverter<TEnumerable, TElement>(
        FormConverter<TElement> elementConverter, 
        IEnumerableTypeShape<TEnumerable, TElement> typeShape) : FormConverter<TEnumerable>
    {
        public override TEnumerable? Read(ref FormDecoder decoder, ReadOnlySpan<char> value)
        {
            var list = new List<TElement>();
            
            foreach (var subRange in value.Split(decoder.m_options.m_nextValueDelimiter))
            {
                var rangeSpan = value[subRange];
                list.Add(elementConverter.Read(ref decoder, rangeSpan)!);
            }
            
            return typeShape.GetEnumerableConstructor()(list);
        }

        public override void Write(ref FormEncoder encoder, TEnumerable? value)
        {
            if (value == null) return;
            
            var enumerable = typeShape.GetGetEnumerable()(value);
            
            var first = true;
            foreach (var element in enumerable)
            {
                if (first) first = false;
                else encoder.m_writer.Append(encoder.m_options.m_nextValueDelimiter);
                elementConverter.Write(ref encoder, element);
            }
        }
    }
}