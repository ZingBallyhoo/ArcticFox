using nietras.SeparatedValues;

namespace ArcticFox.PolyType.Sep.Converters
{
    public class SepObjectConverter<T>(Func<T> defaultConstructor, SepPropertyConverter<T>[] properties) : SepConverter<T>
    {
        private readonly Dictionary<string, SepPropertyConverter<T>> m_propertiesToRead = properties.ToDictionary(x => x.Name, x => x);
        
        public override T Read(SepReader reader)
        {
            // todo: we really should be able to cache property lookups per table
            // but this works for now
            
            var inst = defaultConstructor();

            var currentRow = reader.Current;
            for (var i = 0; i < currentRow.ColCount; i++)
            {
                var columnName = reader.Header.ColNames[i];
                
                if (!m_propertiesToRead.TryGetValue(columnName, out var property))
                {
                    throw new InvalidDataException($"{typeof(T)}: unknown field \"{columnName}\"");
                }
                
                property.Read(currentRow[i].Span, ref inst);
            }
                
            return inst;
        }
    }
}