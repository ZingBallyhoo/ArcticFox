using System;

namespace ArcticFox.RPC.Parameters
{
    public class RpcParameterWithPayloadType<TCallContext, TArgumentState> : RpcParameter<TCallContext, TArgumentState>, IRpcParameterWithPayloadType
    {
        public Type PayloadType { get; set; }
    }
    
    public interface IRpcParameterWithPayloadType : IRpcParameter
    {
        public Type PayloadType { get; set; }
    }
}