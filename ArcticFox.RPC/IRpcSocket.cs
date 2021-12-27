using System.Threading;
using System.Threading.Tasks;

namespace ArcticFox.RPC
{
    public interface IRpcSocket
    {
        ValueTask CallRemoteAsync<TRequest>(RpcMethod method, TRequest request, RpcCallback? callback, CancellationToken cancellationToken=default) where TRequest : class;
    }
}