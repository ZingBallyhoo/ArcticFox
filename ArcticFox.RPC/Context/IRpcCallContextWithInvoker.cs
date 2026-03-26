using ArcticFox.RPC.Parameters;
using PolyType.Abstractions;

namespace ArcticFox.RPC.Context
{
    public interface IRpcCallContextWithInvoker<TCallContext> where TCallContext : IRpcCallContextWithInvoker<TCallContext>
    {
        IRpcInvoker<TCallContext> Invoker { get; set; }

        public static RpcParameter<TCallContext, TArgumentState> BindInvoker<TArgumentState, TParameterType>(IParameterShape<TArgumentState, TParameterType> parameterShape, object? state = null) 
        {
            var setter = parameterShape.GetSetter();
            var getter = parameterShape.GetGetter();
            
            return new RpcParameter<TCallContext, TArgumentState>
            {
                FromContext = delegate(ref TArgumentState argumentState, ref TCallContext context)
                {
                    setter(ref argumentState, (TParameterType)context.Invoker);
                },
                ToContext = delegate(ref TArgumentState argumentState, ref TCallContext context)
                {
                    context.Invoker = (IRpcInvoker<TCallContext>)getter(ref argumentState)!;
                },
            };
        }
    }
}