using System;
using System.Threading;
using System.Threading.Tasks;

namespace ArcticFox.RPC
{
    public abstract class RpcMethod
    {
        public readonly string m_serviceName;
        public readonly string m_methodName;

        protected RpcMethod(string serviceName, string methodName)
        {
            m_serviceName = serviceName;
            m_methodName = methodName;
        }
        
        public abstract object DecodeRequest(ReadOnlySpan<byte> data, object? token);
    }

    public abstract class RpcMethod<TReq> : RpcMethod where TReq : class
    {
        protected RpcMethod(string serviceName, string methodName) : base(serviceName, methodName)
        { }
        
        public ValueTask CallAsync(IRpcSocket rpcSocket, TReq request, CancellationToken cancellationToken=default)
        {
            return rpcSocket.CallRemoteAsync(this, request, null, cancellationToken);
        }
    }

    public abstract class RpcMethod<TReq, TResp> : RpcMethod<TReq>, IResponseDecoder where TReq : class where TResp : class
    {
        protected RpcMethod(string serviceName, string methodName) : base(serviceName, methodName)
        { }
        
        private async ValueTask<TaskCompletionSource<object>> CallCore(IRpcSocket rpcSocket, TReq request, CancellationToken cancellationToken=default)
        {
            var tcs = new TaskCompletionSource<object>();
            var callback = new RpcCallback(this, tcs);
            await rpcSocket.CallRemoteAsync(this, request, callback, cancellationToken);
            return tcs;
        }
    
        public async ValueTask<TResp> Call(IRpcSocket rpcSocket, TReq request, CancellationToken cancellationToken=default)
        {
            var tcs = await CallCore(rpcSocket, request, cancellationToken);
            var response = (TResp)await tcs.Task.WaitAsync(cancellationToken);
            return response;
        }

        public ValueTask CallAsync(IRpcSocket rpcSocket, TReq request, Func<TResp, ValueTask> callbackFunc, CancellationToken cancellationToken=default)
        {
            var callback = new RpcCallback(this, (response) => callbackFunc((TResp) response));
            return rpcSocket.CallRemoteAsync(this, request, callback, cancellationToken);
        }

        public abstract object DecodeResponse(ReadOnlySpan<byte> data, object? token);
    }
}