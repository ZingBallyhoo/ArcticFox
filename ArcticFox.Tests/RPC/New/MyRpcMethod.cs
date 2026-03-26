using ArcticFox.RPC.Methods;
using ArcticFox.RPC.Parameters;

namespace ArcticFox.Tests.RPC.New
{
    public class MyRpcMethod : IRpcMethod, IRpcMethodInvokable<MyRpcCallContext>, IRpcCallContextFactory<MyRpcCallContext>
    {
        public object FromShape { get; set; }
        public IRpcParameter[] Parameters { get; set; }
        
        public BoundRpcMethod3<MyRpcCallContext> Invoke { get; set; }
        
        public MyRpcCallContext CreateCallContext()
        {
            return new MyRpcCallContext();
        }
    }
}