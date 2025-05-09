namespace ArcticFox.PolyType.Amf.Zero
{
    public class Amf0BoolConverter : AmfConverter<bool>
    {
        public override void Write(ref AmfEncoder encoder, bool value)
        {
            encoder.PutMarker(Amf0TypeMarker.Boolean);
            encoder.PutUInt8(value ? (byte)1 : (byte)0);
        }

        public override bool Read(ref AmfDecoder decoder)
        {
            var marker = decoder.ReadMarker();
            if (marker != Amf0TypeMarker.Boolean)
            {
                throw new NotImplementedException($"unknown boolean marker: {marker}");
            }
            
            return decoder.ReadBool();
        }
    }
}