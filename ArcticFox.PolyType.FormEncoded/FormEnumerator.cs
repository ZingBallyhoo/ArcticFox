namespace ArcticFox.PolyType.FormEncoded.Converters
{
    public ref struct FormEnumerator
    {
        private readonly ReadOnlySpan<char> m_text;
        private readonly char m_keyValueDelimiter;
        private MemoryExtensions.SpanSplitEnumerator<char> m_enumerator;
        
        public FormVarRange Current { get; set; }
        
        public struct FormVarRange
        {
            public required Range m_name;
            public required Range m_value;
        }
        
        public FormEnumerator(ReadOnlySpan<char> text, char propertyDelimiter, char keyValueDelimiter)
        {
            m_text = text;
            m_keyValueDelimiter = keyValueDelimiter;
            m_enumerator = m_text.Split(propertyDelimiter);
        }
        
        public bool MoveNext()
        {
            if (!m_enumerator.MoveNext())
            {
                return false;
            }
            
            var varRange = m_enumerator.Current;
            var varSpan = m_text[varRange];
            if (varSpan.Length == 0) return false; // ignore trailing
            
            var indexOfNameDelimiter = varSpan.IndexOf(m_keyValueDelimiter);
            if (indexOfNameDelimiter == -1) throw new InvalidDataException("cant find name-value delimiter");
            
            var nameEndIndex = new Index(varRange.Start.Value + indexOfNameDelimiter);
            var valueStartIndex = new Index(nameEndIndex.Value+1);
            
            var nameRange = new Range(varRange.Start, nameEndIndex);
            var valueRange = new Range(valueStartIndex, varRange.End);
            Current = new FormVarRange
            {
                m_name = nameRange,
                m_value = valueRange
            };
            
            return true;
        }
        
        public FormEnumerator GetEnumerator() => this;
    }
}