namespace ArcticFox.RPC.Methods
{
    public interface IRpcCallContextFactory<out TCallContext>
    {
        TCallContext CreateCallContext();
    }
}