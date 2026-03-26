using System.Threading;
using ArcticFox.Net;
using ArcticFox.RPC;
using ArcticFox.RPC.Context;

namespace ArcticFox.Tests.RPC.New
{
    public class MyRpcCallContext : IRpcCallContextWithCancellation, IRpcCallContextWithPayload, IRpcCallContextWithSocket, IRpcCallContextWithCompletion, IRpcCallContextWithInvoker<MyRpcCallContext>
    {
        public CancellationToken CancellationToken { get; set; }
        public object? Payload { get; set; }
        public HighLevelSocket Socket { get; set; }
        
        public IRpcResultReceiver? Completion { get; set; }
        public IRpcInvoker<MyRpcCallContext> Invoker { get; set; }
    }

    //public struct MyRpcCompletionCallContext : IRpcCallContextWithPayload
    //{
    //    public object? Payload { get; set; }
    //}
}