namespace ArcticFox.PolyType.FormEncoded.Converters
{
    internal class FormDictConverter<TDictionary, TKey, TValue>(
        FormConverter<TKey> keyConverter, 
        FormConverter<TValue> valueConverter, 
        Func<TDictionary, IReadOnlyDictionary<TKey, TValue>> getDictionary) : FormConverter<TDictionary>
    {
        public override TDictionary? Read(ref FormDecoder decoder, ReadOnlySpan<char> value)
        {
            throw new NotImplementedException();
        }

        public override void Write(ref FormEncoder encoder, TDictionary? value)
        {
            if (value == null) return;
            
            var dictionary = getDictionary(value);
            
            var first = true;
            foreach (var pair in dictionary)
            {
                if (first) first = false;
                else encoder.m_writer.Append(encoder.m_options.m_nextPropertyDelimiter);
                
                keyConverter.Write(ref encoder, pair.Key);
                encoder.m_writer.Append(encoder.m_options.m_keyValueDelimiter);
                valueConverter.Write(ref encoder, pair.Value);
            }
        }
    }
}