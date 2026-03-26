namespace ArcticFox.RPC.Parameters
{
    public class RpcParameter<TCallContext, TArgumentState> : IRpcParameter
    {
        public required RpcParameterAccessor<TCallContext, TArgumentState> FromContext;
        public required RpcParameterAccessor<TCallContext, TArgumentState> ToContext;
    }
}