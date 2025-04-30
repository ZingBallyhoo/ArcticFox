namespace ArcticFox.PolyType.FormEncoded.Converters
{
    public class FormObjectConverter<T>(Func<T> defaultConstructor, FormPropertyConverter<T>[] properties) : FormConverter<T>
    {
        private readonly Dictionary<string, FormPropertyConverter<T>> m_propertiesToRead = properties.Where(prop => prop.HasGetter).ToDictionary(x => x.Name, x => x);
        private readonly FormPropertyConverter<T>[] m_propertiesToWrite = properties.Where(prop => prop.HasSetter).ToArray();
        
        public override T? Read(ref FormDecoder decoder, ReadOnlySpan<char> value)
        {
            var inst = defaultConstructor();

            foreach (var formRange in new FormEnumerator(value, decoder.m_options.m_nextPropertyDelimiter, decoder.m_options.m_keyValueDelimiter))
            {
                var nameSpan = value[formRange.m_name];
                var valueSpan = value[formRange.m_value];
                
                var decodedNameSpan = decoder.DecodeKey(nameSpan);
                if (!m_propertiesToRead.GetAlternateLookup<ReadOnlySpan<char>>().TryGetValue(decodedNameSpan, out var property))
                {
                    if (!decoder.m_options.m_throwOnUnknownFields) continue;
                    throw new InvalidDataException($"{typeof(T)}: unknown field \"{nameSpan}\"");
                }
                    
                property.Read(ref decoder, valueSpan, ref inst);
            }
                
            return inst;
        }
        
        public override void Write(ref FormEncoder encoder, T? value)
        {
            foreach (var property in m_propertiesToWrite)
            {
                encoder.WriteEncodedKey(property.Name);
                encoder.m_writer.Append(encoder.m_options.m_keyValueDelimiter);
                property.Write(ref encoder, ref value!);
            }
        }
    }
}