using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ArcticFox.RPC.Context;
using ArcticFox.RPC.Methods;
using ArcticFox.RPC.Parameters;
using PolyType;
using PolyType.Abstractions;

namespace ArcticFox.RPC.Binding
{
    public class RpcResolverByPayload<TMethod, TCallContext> : IRpcInvoker<TCallContext> 
        where TMethod: IRpcMethodInvokable<TCallContext>, IRpcMethod
        where TCallContext : IRpcCallContextWithCancellation, IRpcCallContextWithPayload
    {
        private readonly TypeShapeVisitor m_visitor;
        private readonly Dictionary<Type, ServiceMethod> m_methods = [];
        
        private class ServiceMethod
        {
            public required Func<TCallContext, object?>? m_resolveImplementation;
            public required TMethod m_method;
        }
        
        public RpcResolverByPayload(TypeShapeVisitor visitor)
        {
            m_visitor = visitor;
        }

        public TMethod[] BindService(ITypeShape shape, Func<TCallContext, object>? resolveImplementation)
        {
            var methods = (TMethod[])shape.Accept(m_visitor)!;

            foreach (var method in methods)
            {
                var payloadParameters = method.Parameters.OfType<IRpcParameterWithPayloadType>().ToArray();
                if (payloadParameters.Length == 0)
                {
                    throw new InvalidDataException($"method {method} has no payload type");
                }
                if (payloadParameters.Length > 1)
                {
                    throw new InvalidDataException($"method {method} has {payloadParameters.Length} payload type params");
                }

                var payloadParameter = payloadParameters.Single();
                m_methods.Add(payloadParameter.PayloadType, new ServiceMethod
                {
                    m_resolveImplementation = resolveImplementation,
                    m_method = method
                });
            }

            return methods;
        }

        public object BindClientService(ITypeShape shape, TypeShapeVisitor clientVisitor)
        {
            var methods = BindService(shape, static _ => throw new NotImplementedException("trying to resolve a client service"));
            return shape.Accept(clientVisitor, methods)!;
        }
        
        public async ValueTask<object?> InvokeAsync(TCallContext callContext)
        {
            var payloadType = callContext.Payload?.GetType();
            if (payloadType == null)
            {
                throw new NullReferenceException("can't bind by payload if payload is null");
            }

            if (!m_methods.TryGetValue(payloadType, out var method))
            {
                throw new InvalidDataException($"no method found for payload type: {payloadType}");
            }
            
            var implementation = method.m_resolveImplementation?.Invoke(callContext);
            var result = await method.m_method.Invoke(implementation, callContext);
            return result;
        }
    }
}