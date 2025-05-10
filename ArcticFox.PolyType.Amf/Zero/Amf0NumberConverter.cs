using System.Numerics;

namespace ArcticFox.PolyType.Amf.Zero
{
    public class Amf0NumberConverter<T> : AmfConverter<T> where T : unmanaged, INumberBase<T>
    {
        public override void Write(ref AmfEncoder encoder, T value)
        {
            encoder.PutMarker(Amf0TypeMarker.Number);
            encoder.PutDouble(double.CreateChecked(value));
        }

        public override T Read(ref AmfDecoder decoder)
        {
            var marker = decoder.ReadMarker();
            if (marker != Amf0TypeMarker.Number)
            {
                throw new NotImplementedException($"unknown number) marker: {marker}");
            }
            
            var doubleValue = decoder.ReadDouble();
            // todo: validate this is a whole number if T is integer
            return T.CreateChecked(doubleValue);
        }
    }
}