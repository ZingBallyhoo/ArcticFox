using System.Threading;
using ArcticFox.RPC.Parameters;
using PolyType.Abstractions;

namespace ArcticFox.RPC.Context
{
    public interface IRpcCallContextWithCancellation
    {
        CancellationToken CancellationToken { get; set; }
        
        public static RpcParameter<TCallContext, TArgumentState> BindCancellationToken<TCallContext, TArgumentState, TParameterType>(IParameterShape<TArgumentState, TParameterType> parameterShape, object? state = null) where TCallContext : IRpcCallContextWithCancellation
        {
            var tokenSetter = (Setter<TArgumentState, CancellationToken>)(object)parameterShape.GetSetter();
            var tokenGetter = (Getter<TArgumentState, CancellationToken>)(object)parameterShape.GetGetter();
                
            return new RpcParameter<TCallContext, TArgumentState>
            {
                FromContext = delegate(ref TArgumentState argumentState, ref TCallContext callContext)
                {
                    tokenSetter(ref argumentState, callContext.CancellationToken);
                },
                ToContext = delegate(ref TArgumentState argumentState, ref TCallContext callContext)
                {
                    callContext.CancellationToken = tokenGetter(ref argumentState);
                }
            };
        }
    }
}