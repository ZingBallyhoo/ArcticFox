using System.Threading;
using System.Threading.Tasks;
using ArcticFox.RPC.Context;
using ArcticFox.RPC.Methods;
using ArcticFox.RPC.Parameters;
using PolyType.Abstractions;

namespace ArcticFox.RPC.Binding
{
    public class RpcClientImplementationBinder<TMethod, TCallContext> : IRpcFakeImplementationBinder<TMethod, TCallContext>
        where TCallContext : IRpcCallContextWithInvoker<TCallContext>
        where TMethod : IRpcCallContextFactory<TCallContext>
    {
        public TFunction BindFunction<TFunction, TArgumentState, TResult>(IFunctionTypeShape<TFunction, TArgumentState, TResult> functionShape, TMethod method, RpcParameter<TCallContext, TArgumentState>[] parameters) where TArgumentState : IArgumentState
        {
            return functionShape.FromAsyncDelegate((ref argState) =>
            {
                var callContext = method.CreateCallContext();
                foreach (var parameter in parameters)
                {
                    parameter.ToContext(ref argState, ref callContext);
                }

                // todo: how can this be decoupled?
                RpcResultReceiver_Task? inlineCompletion = null;
                var cancellationToken = CancellationToken.None;
                if (callContext is IRpcCallContextWithCompletion withCompletion && !functionShape.IsVoidLike)
                {
                    withCompletion.Completion ??= inlineCompletion = new RpcResultReceiver_Task();
                }
                if (inlineCompletion != null && callContext is IRpcCallContextWithCancellation withCancellation)
                {
                    cancellationToken = withCancellation.CancellationToken;
                }

                var invokeTask = callContext.Invoker.InvokeAsync(callContext);
                return Completion(invokeTask);
                
                async ValueTask<TResult> Completion(ValueTask<object?> invokeTask)
                {
                    var rawResult = await invokeTask;
                    if (inlineCompletion != null)
                    {
                        rawResult = await inlineCompletion.TaskCompletionSource.Task.WaitAsync(cancellationToken);
                    }

                    if (functionShape.IsVoidLike)
                    {
                        // .....
                        return default!;
                    }
                    return (TResult)rawResult!;
                }
            });
        }
    }
}