using ArcticFox.RPC.Parameters;

namespace ArcticFox.RPC.Methods
{
    public interface IRpcMethod
    {
        public object FromShape { get; set; }
        public IRpcParameter[] Parameters { get; set; }
    }
}