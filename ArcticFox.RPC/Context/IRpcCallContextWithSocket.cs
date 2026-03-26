using ArcticFox.Net;
using ArcticFox.RPC.Parameters;
using PolyType.Abstractions;

namespace ArcticFox.RPC.Context
{
    public interface IRpcCallContextWithSocket
    {
        HighLevelSocket? Socket { get; set; }
        
        public static RpcParameter<TCallContext, TArgumentState> BindSocket<TCallContext, TArgumentState, TParameterType>(IParameterShape<TArgumentState, TParameterType> parameterShape, object? state = null) where TCallContext : IRpcCallContextWithSocket
        {
            var tokenSetter = parameterShape.GetSetter();
            var tokenGetter = parameterShape.GetGetter();
            return new RpcParameter<TCallContext, TArgumentState>
            {
                FromContext = (ref argumentState, ref callContext) =>
                {
                    tokenSetter(ref argumentState, (TParameterType)(object)callContext.Socket!);
                },
                ToContext = (ref argumentState, ref callContext) =>
                {
                    callContext.Socket = (HighLevelSocket?)(object?)tokenGetter(ref argumentState);
                }
            };
        }
    }
}