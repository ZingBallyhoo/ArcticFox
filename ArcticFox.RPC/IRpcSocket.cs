using System.Threading.Tasks;

namespace ArcticFox.RPC
{
    public interface IRpcSocket
    {
        ValueTask CallRemoteAsync<T>(RpcMethod method, T request, RpcCallback? callback) where T : class;
    }
}