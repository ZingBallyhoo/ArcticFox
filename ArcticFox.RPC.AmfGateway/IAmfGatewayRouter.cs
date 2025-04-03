namespace ArcticFox.RPC.AmfGateway
{
    public interface IAmfGatewayRouter
    {
        ValueTask<object> RouteRequest(AmfGatewayContext context, CancellationToken cancellationToken);
    }
}