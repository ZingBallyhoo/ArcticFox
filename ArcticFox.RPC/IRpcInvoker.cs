using System.Threading.Tasks;

namespace ArcticFox.RPC
{
    public interface IRpcInvoker<in TCallContext>
    {
        ValueTask<object?> InvokeAsync(TCallContext callContext);
    }
}