namespace ArcticFox.RPC.Parameters
{
    public delegate void RpcParameterAccessor<TCallContext, TArgumentState>(ref TArgumentState argumentState, ref TCallContext callContext);
}