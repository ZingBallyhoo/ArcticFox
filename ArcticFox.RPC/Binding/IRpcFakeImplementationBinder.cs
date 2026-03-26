using ArcticFox.RPC.Parameters;
using PolyType.Abstractions;

namespace ArcticFox.RPC.Binding
{
    public interface IRpcFakeImplementationBinder<in TMethod, TCallContext>
    {
        TFunction BindFunction<TFunction, TArgumentState, TResult>(
            IFunctionTypeShape<TFunction, TArgumentState, TResult> functionShape,
            TMethod method,
            RpcParameter<TCallContext, TArgumentState>[] parameters) where TArgumentState : IArgumentState;
    }
}