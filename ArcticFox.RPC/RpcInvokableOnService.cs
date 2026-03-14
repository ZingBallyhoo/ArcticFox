using System;
using ArcticFox.RPC.Methods;

namespace ArcticFox.RPC
{
    public class RpcInvokableOnService<TCallContext>
    {
        public Func<TCallContext, object?>? m_resolveImplementation;
        public IRpcMethodInvokable<TCallContext> m_invokable;
    }
}