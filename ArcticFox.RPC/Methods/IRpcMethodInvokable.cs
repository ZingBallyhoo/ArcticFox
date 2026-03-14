using System.Threading.Tasks;

namespace ArcticFox.RPC.Methods
{
    public delegate Task<object?> BoundRpcMethod3<in TContext>(object? target, TContext callContext) where TContext : allows ref struct;
    
    public interface IRpcMethodInvokable<TCallContext>
    {
        BoundRpcMethod3<TCallContext> Invoke { get; set; }
    }
}