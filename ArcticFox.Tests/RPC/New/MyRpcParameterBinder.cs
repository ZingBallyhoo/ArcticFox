using System.Threading;
using ArcticFox.Net;
using ArcticFox.RPC;
using ArcticFox.RPC.Context;
using ArcticFox.RPC.Parameters;
using PolyType.Abstractions;

namespace ArcticFox.Tests.RPC.New
{
    public class MyRpcParameterBinder : IRpcParameterBinder<MyRpcCallContext>
    {
        public RpcParameter<MyRpcCallContext, TArgumentState>? VisitImplicitParameter<TArgumentState, TParameterType>(IParameterShape<TArgumentState, TParameterType> parameterShape, object? state = null)
        {
            if (parameterShape.ParameterType.Type == typeof(CancellationToken))
            {
                return IRpcCallContextWithCancellation.BindCancellationToken<MyRpcCallContext, TArgumentState, TParameterType>(parameterShape, state);
            }
            if (parameterShape.ParameterType.Type.IsAssignableTo(typeof(IRpcInvoker<MyRpcCallContext>)))
            {
                return IRpcCallContextWithInvoker<MyRpcCallContext>.BindInvoker(parameterShape, state);
            }
            if (parameterShape.ParameterType.Type.IsAssignableTo(typeof(HighLevelSocket)))
            {
                return IRpcCallContextWithSocket.BindSocket<MyRpcCallContext, TArgumentState, TParameterType>(parameterShape, state);
            }

            return null;
        }

        public RpcParameter<MyRpcCallContext, TArgumentState>? VisitExplicitParameter<TArgumentState, TParameterType>(IParameterShape<TArgumentState, TParameterType> parameterShape, object? state = null)
        {
            return IRpcCallContextWithPayload.BindPayload<MyRpcCallContext, TArgumentState, TParameterType>(parameterShape, state);
        }
    }
}