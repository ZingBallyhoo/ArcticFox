namespace ArcticFox.RPC.Context
{
    public interface IRpcCallContextWithCompletion
    {
        IRpcResultReceiver? Completion { get; set; }
    }
}