namespace ArcticFox.PolyType.Amf.Zero
{
    public class Amf0IntConverter : AmfConverter<int>
    {
        public override void Write(ref AmfEncoder encoder, int value)
        {
            encoder.PutMarker(Amf0TypeMarker.Number);
            encoder.PutDouble(value);
        }

        public override int Read(ref AmfDecoder decoder)
        {
            var marker = decoder.ReadMarker();
            if (marker != Amf0TypeMarker.Number)
            {
                throw new NotImplementedException($"unknown number(int) marker: {marker}");
            }
            
            var doubleValue = decoder.ReadDouble();
            // todo: validate that its a whole number
            return (int)doubleValue;
        }
    }
}