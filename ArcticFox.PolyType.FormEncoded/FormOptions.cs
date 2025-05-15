using System.Text;
using PolyType;
using PolyType.Utilities;

namespace ArcticFox.PolyType.FormEncoded
{
    public class FormOptions
    {
        public static FormOptions Default = new FormOptions();
        
        public bool m_encodeKeys = true;
        public bool m_encodeValues = true;
        public bool m_encodeSpacesAsPlus = false;
        
        public char m_keyValueDelimiter = '=';
        public char m_nextValueDelimiter = ',';
        public char m_nextPropertyDelimiter = '&';
        
        public bool m_throwOnUnknownFields;
        
        private static readonly MultiProviderTypeCache m_cache = new MultiProviderTypeCache
        {
            ValueBuilderFactory = ctx => new FormBuilder(ctx)
            // todo: delay
        };
        
        public T Deserialize<T>(string text) where T : IShapeable<T>
        {
            return Deserialize<T>(text.AsSpan());
        }
        
        public T Deserialize<T>(ReadOnlySpan<char> text) where T : IShapeable<T>
        {
            var converter = (FormConverter<T>)m_cache.GetOrAdd(T.GetShape())!;
            
            var decoder = new FormDecoder
            {
                m_options = this
            };
            return converter.Read(ref decoder, text)!;
        }
        
        public string Serialize2<T, TProvider>(T? value) where TProvider : IShapeable<T>
        {
            var converter = (FormConverter<T>)m_cache.GetOrAdd(TProvider.GetShape())!;
            
            var encoder = new FormEncoder
            {
                m_options = this,
                m_writer = new StringBuilder()
            };
            converter.Write(ref encoder, value);
            return encoder.m_writer.ToString();
        }
        
        public string Serialize<T>(T value) where T : IShapeable<T>
        {
            return Serialize2<T, T>(value);
        }
    }
    
    public struct FormEncoder
    {
        public required StringBuilder m_writer;
        public required FormOptions m_options;
        
        public void WriteEncodedKey(ReadOnlySpan<char> key)
        {
            if (!m_options.m_encodeKeys)
            {
                m_writer.Append(key);
                return;
            }
            
            WriteEncoded(key);
        }
        
        public void WriteEncodedValue(ReadOnlySpan<char> value)
        {
            if (!m_options.m_encodeValues)
            {
                m_writer.Append(value);
                return;
            }
            
            WriteEncoded(value);
        }
        
        private void WriteEncoded(ReadOnlySpan<char> data)
        {
            if (data.IndexOfAnyExcept(UriHelper.UnreservedReserved) == -1)
            {
                // if we got here, definitely cant contain a space (so no need to check)
                m_writer.Append(data);
                return;
            }
            
            var dataString = Uri.EscapeDataString(data);
            if (m_options.m_encodeSpacesAsPlus)
            {
                dataString = dataString.Replace("%20", "+");
            }
            m_writer.Append(dataString);
        }
    }
    
    public ref struct FormDecoder
    {
        public required FormOptions m_options;
        
        public ReadOnlySpan<char> DecodeKey(ReadOnlySpan<char> input)
        {
            if (!m_options.m_encodeKeys) return input;
            return Decode(input);
        }
        
        public ReadOnlySpan<char> DecodeValue(ReadOnlySpan<char> input)
        {
            if (!m_options.m_encodeValues) return input;
            return Decode(input);
        }
        
        private ReadOnlySpan<char> Decode(ReadOnlySpan<char> input)
        {
            var needSpaceDecode = m_options.m_encodeSpacesAsPlus && input.Contains('+');
            if (!needSpaceDecode && !input.Contains('%'))
            {
                return input;
            }
            
            var dataString = Uri.UnescapeDataString(input);
            if (m_options.m_encodeSpacesAsPlus)
            {
                dataString = dataString.Replace('+', ' ');
            }
            return dataString;
        }
    }
}