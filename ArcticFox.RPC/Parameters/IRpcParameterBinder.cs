using PolyType.Abstractions;

namespace ArcticFox.RPC.Parameters
{
    public interface IRpcParameterBinder<TCallContext>
    {
        RpcParameter<TCallContext, TArgumentState>? VisitImplicitParameter<TArgumentState, TParameterType>(IParameterShape<TArgumentState, TParameterType> parameterShape, object? state = null);
        RpcParameter<TCallContext, TArgumentState>? VisitExplicitParameter<TArgumentState, TParameterType>(IParameterShape<TArgumentState, TParameterType> parameterShape, object? state = null);
    }
}