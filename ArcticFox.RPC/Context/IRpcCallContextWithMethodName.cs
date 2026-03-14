namespace ArcticFox.RPC.Context
{
    public interface IRpcCallContextWithMethodName
    {
        string ServiceName { get; set; }
        string MethodName { get; set; }
    }
}