using System;
using System.Threading;
using System.Threading.Tasks;

namespace ArcticFox.RPC.Legacy
{
    public interface IService<TSocket>
    {
        ValueTask<object?> InvokeMethodHandler(TSocket socket, string method, ReadOnlySpan<byte> data, object? token, CancellationToken cancellationToken=default);
    }
}