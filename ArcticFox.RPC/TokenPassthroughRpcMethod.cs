using System;

namespace ArcticFox.RPC
{
    public class TokenPassthroughRpcMethod<TReq> : RpcMethod<TReq> where TReq : class
    {
        public TokenPassthroughRpcMethod(string serviceName, string methodName) : base(serviceName, methodName)
        { }

        public override TReq DecodeRequest(ReadOnlySpan<byte> data, object? token)
        {
            if (token == null) throw new ArgumentNullException(nameof(token));
            return (TReq)token;
        }
    }

    public class TokenPassthroughRpcMethod<TReq, TResp> : RpcMethod<TReq, TResp> where TReq : class where TResp : class
    {
        public TokenPassthroughRpcMethod(string serviceName, string methodName) : base(serviceName, methodName)
        { }

        public override TReq DecodeRequest(ReadOnlySpan<byte> data, object? token)
        {
            if (token == null) throw new ArgumentNullException(nameof(token));
            return (TReq)token;
        }

        public override TResp DecodeResponse(ReadOnlySpan<byte> data, object? token)
        {
            if (token == null) throw new ArgumentNullException(nameof(token));
            return (TResp)token;
        }
    }
}