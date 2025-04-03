using PolyType;

namespace ArcticFox.PolyType.Amf.Packet
{
    [GenerateShape]
    public partial class AmfPacket
    {
        public AmfVersion m_version = AmfVersion.Zero;
        public List<AmfHeader> m_headers = new List<AmfHeader>();
        public List<AmfMessage> m_messages = new List<AmfMessage>();
    }
}