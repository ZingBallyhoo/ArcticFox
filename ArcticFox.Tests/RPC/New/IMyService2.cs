using System;
using System.Threading;
using System.Threading.Tasks;
using PolyType;

namespace ArcticFox.Tests.RPC.New
{
    public interface IMyService2<TSocket>
    {
        public delegate ValueTask<Response1> OneDelegate(TSocket socket, Request1 request, CancellationToken cancellationToken);
        public delegate ValueTask<Response2> TwoDelegate(TSocket socket, Request2 request, CancellationToken cancellationToken);
        public delegate ValueTask ThreeDelegate(TSocket socket, Request3 request, CancellationToken cancellationToken);
        public delegate ValueTask<Response4> FourDelegate(TSocket socket, Request4 request, CancellationToken cancellationToken);
        //Func<TSocket, Request1, CancellationToken> One { get; set; }

        public OneDelegate One { get; set; }
        public TwoDelegate Two { get; set; }
        public ThreeDelegate Three { get; set; }
        public FourDelegate Four { get; set; }
    }
    
    public abstract class MyService2_ImplBase<TSocket> : IMyService2<TSocket>
    {
        public IMyService2<TSocket>.OneDelegate One { get; set; }
        public IMyService2<TSocket>.TwoDelegate Two { get; set; }
        public IMyService2<TSocket>.ThreeDelegate Three { get; set; }
        public IMyService2<TSocket>.FourDelegate Four { get; set; }

        public MyService2_ImplBase()
        {
            One = OneImpl;
            Two = TwoImpl;
            Three = ThreeImpl;
            Four = FourImpl;
        }

        public abstract ValueTask<Response1> OneImpl(TSocket socket, Request1 request, CancellationToken cancellationToken);
        public abstract ValueTask<Response2> TwoImpl(TSocket socket, Request2 request, CancellationToken cancellationToken);
        public abstract ValueTask ThreeImpl(TSocket socket, Request3 request, CancellationToken cancellationToken);
        public abstract ValueTask<Response4> FourImpl(TSocket socket, Request4 request, CancellationToken cancellationToken);
    }
    
    [GenerateShapeFor<MyService2_Server>]
    [GenerateShapeFor<IMyService2<MyRpcServerSocket2>>]
    [TypeShape(IncludeMethods = MethodShapeFlags.PublicInstance)]
    public partial class MyService2_Server : MyService2_ImplBase<MyRpcServerSocket2>
    {
        public override ValueTask<Response1> OneImpl(MyRpcServerSocket2 socket, Request1 request, CancellationToken cancellationToken)
        {
            return new ValueTask<Response1>(new Response1());
        }

        public override ValueTask<Response2> TwoImpl(MyRpcServerSocket2 socket, Request2 request, CancellationToken cancellationToken)
        {
            return new ValueTask<Response2>(new Response2());
        }

        public override ValueTask ThreeImpl(MyRpcServerSocket2 socket, Request3 request, CancellationToken cancellationToken)
        {
            return ValueTask.CompletedTask;
        }

        public override ValueTask<Response4> FourImpl(MyRpcServerSocket2 socket, Request4 request, CancellationToken cancellationToken)
        {
            throw new Exception("oh no....");
        }
    }
}