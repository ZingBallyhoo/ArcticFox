using ArcticFox.RPC.Parameters;
using PolyType.Abstractions;

namespace ArcticFox.RPC.Context
{
    public interface IRpcCallContextWithPayload
    {
        object? Payload { get; set; }
        
        public static RpcParameterWithPayloadType<TCallContext, TArgumentState> BindPayload<TCallContext, TArgumentState, TParameterType>(IParameterShape<TArgumentState, TParameterType> parameterShape, object? state = null) where TCallContext : IRpcCallContextWithPayload
        {
            var tokenSetter = parameterShape.GetSetter();
            var tokenGetter = parameterShape.GetGetter();
            return new RpcParameterWithPayloadType<TCallContext, TArgumentState>
            {
                FromContext = (ref argumentState, ref callContext) =>
                {
                    tokenSetter(ref argumentState, (TParameterType)callContext.Payload!);
                },
                ToContext = (ref argumentState, ref callContext) =>
                {
                    callContext.Payload = tokenGetter(ref argumentState);
                },
                PayloadType = parameterShape.ParameterType.Type
            };
        }
    }
}