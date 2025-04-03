using ArcticFox.PolyType.Amf;
using PolyType;

namespace ArcticFox.RPC.AmfGateway
{
    public class AmfGatewaySettings
    {
        public required AmfOptions m_options;
        public required ITypeShapeProvider m_shapeProvider;
        
        public bool m_swallowExceptions;
    }
}