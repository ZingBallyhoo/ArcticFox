using ArcticFox.RPC.Binding;
using ArcticFox.RPC.Methods;
using ArcticFox.RPC.Parameters;

namespace ArcticFox.Tests.RPC.New
{
    public class MyRpcResolver : RpcResolverByPayload<MyRpcMethod, MyRpcCallContext>
    {
        public MyRpcResolver() : base(new RpcCollectMethodsVisitor<MyRpcMethod>(
            new RpcParameterVisitor<MyRpcCallContext>(new MyRpcParameterBinder()),
            new RpcMethodVisitor<MyRpcMethod>(),
            new RpcImplementationVisitor<MyRpcCallContext>()))
        {
        }
    }
}