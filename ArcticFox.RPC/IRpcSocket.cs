using System.Threading.Tasks;

namespace ArcticFox.RPC
{
    public interface IRpcSocket
    {
        ValueTask CallRemoteAsync(RpcMethod method, object request, RpcCallback? callback);
    }
}