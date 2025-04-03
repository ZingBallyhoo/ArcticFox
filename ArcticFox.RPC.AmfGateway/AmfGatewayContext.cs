using ArcticFox.PolyType.Amf.Packet;
using Microsoft.AspNetCore.Http;

namespace ArcticFox.RPC.AmfGateway
{
    public class AmfGatewayContext
    {
        public required HttpContext m_httpContext;
        
        public required AmfPacket m_requestPacket;
        public required AmfPacket m_responsePacket;
        
        public required AmfMessage m_message;
    }
}