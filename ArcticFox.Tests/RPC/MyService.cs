using System;
using System.Threading;
using System.Threading.Tasks;
using ArcticFox.RPC;

namespace ArcticFox.Tests.RPC
{
    [RpcMethod(typeof(TokenPassthroughRpcMethod<Request1, Response1>), "One")]
    [RpcMethod(typeof(TokenPassthroughRpcMethod<Request2, Response2>), "Two")]
    [RpcMethod(typeof(TokenPassthroughRpcMethod<Request3>), "Three")]
    [RpcMethod(typeof(TokenPassthroughRpcMethod<Request4, Response4>), "Four")]
    public abstract partial class MyService
    {
    }
    
    public class MyService_Server : MyService<MyRpcServerSocket>
    {
        public override ValueTask<Response1> One(MyRpcServerSocket socket, Request1 request, CancellationToken cancellationToken=default)
        {
            return new ValueTask<Response1>(new Response1());
        }

        public override ValueTask<Response2> Two(MyRpcServerSocket socket, Request2 request, CancellationToken cancellationToken=default)
        {
            return new ValueTask<Response2>(new Response2());
        }

        public override ValueTask Three(MyRpcServerSocket socket, Request3 request, CancellationToken cancellationToken=default)
        {
            return ValueTask.CompletedTask;
        }

        public override ValueTask<Response4> Four(MyRpcServerSocket socket, Request4 request, CancellationToken cancellationToken=default)
        {
            throw new Exception("oh no....");
        }
    }
}