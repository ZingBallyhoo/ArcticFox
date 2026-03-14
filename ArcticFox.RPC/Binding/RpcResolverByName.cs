using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ArcticFox.RPC.Context;
using ArcticFox.RPC.Methods;
using PolyType;
using PolyType.Abstractions;

namespace ArcticFox.RPC.Binding
{
    public class RpcResolverByName<TMethod, TCallContext> : IRpcInvoker<TCallContext> 
        where TMethod: IRpcMethodInvokable<TCallContext>, IRpcMethodWithName
        where TCallContext : IRpcCallContextWithMethodName
    {
        private readonly TypeShapeVisitor m_visitor;
        private readonly Dictionary<string, Service> m_services = [];
        
        private class Service
        {
            public readonly Dictionary<string, RpcInvokableOnService<TCallContext>> m_methods = [];
        }

        public RpcResolverByName(TypeShapeVisitor visitor)
        {
            m_visitor = visitor;
        }
        
        public void BindService(string serviceName, ITypeShape shape, Func<TCallContext, object>? resolveImplementation)
        {
            var service = new Service();
            
            var methods = (TMethod[])shape.Accept(m_visitor)!;
            foreach (var method in methods)
            {
                method.ServiceName = serviceName;
                
                service.m_methods.Add(method.MethodName, new RpcInvokableOnService<TCallContext>
                {
                    m_resolveImplementation = resolveImplementation,
                    m_invokable = method
                });
            }

            m_services.Add(serviceName, service);
        }

        public async ValueTask<object?> InvokeAsync(TCallContext callContext)
        {
            if (!m_services.TryGetValue(callContext.ServiceName, out var serviceDef))
            {
                throw new InvalidDataException($"no service with name: \"{callContext.ServiceName}\"");
            }

            if (!serviceDef.m_methods.TryGetValue(callContext.MethodName, out var method))
            {
                throw new InvalidDataException($"no method with name: \"{callContext.MethodName}\"");
            }

            var implementation = method.m_resolveImplementation?.Invoke(callContext);
            var result = await method.m_invokable.Invoke(implementation, callContext);
            return result;
        }
    }
}