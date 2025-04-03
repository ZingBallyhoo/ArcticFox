using System.Diagnostics.CodeAnalysis;
using ArcticFox.PolyType.Amf;
using ArcticFox.PolyType.Amf.Packet;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace ArcticFox.RPC.AmfGateway
{
    public static class AmfGatewayEndpointRouteBuilderExtensions
    {
        private const string CONTENT_TYPE = "application/x-amf";
        
        // todo: we should provide a default router implementation that can map to multiple rpc services
        // but there's no good way to make it work yet...
        // is it also a problem if multiple implementations are required in one app?
        // could be moved to settings
        
        public static IEndpointConventionBuilder MapAmfGateway(this IEndpointRouteBuilder endpoints, [StringSyntax("Route")] string pattern, AmfGatewaySettings settings)
        {
            return endpoints.MapPost(pattern, async (ILogger<AmfGatewayContext> logger, IAmfGatewayRouter router, HttpContext httpContext) =>
            {
                var cancellationToken = httpContext.RequestAborted;
                if (httpContext.Request.ContentLength == null) return Results.InternalServerError();
            
                var body = new byte[httpContext.Request.ContentLength.Value];
                await httpContext.Request.Body.ReadExactlyAsync(body, cancellationToken);
            
                var requestPacket = AmfPolyType.Deserialize<AmfPacket>(body, settings.m_options, settings.m_shapeProvider);
                var responsePacket = new AmfPacket();
                
                foreach (var message in requestPacket.m_messages)
                {
                    var gatewayContext = new AmfGatewayContext
                    {
                        m_httpContext = httpContext,
                        m_requestPacket = requestPacket,
                        m_responsePacket = responsePacket,
                        m_message = message
                    };
                    
                    logger.LogTrace("Incoming AMF request: {TargetUri}", message.m_targetUri);
                    
                    bool success;
                    object? serviceResult;
                    try
                    {
                        serviceResult = await router.RouteRequest(gatewayContext, cancellationToken);
                        //serviceResult = await handler(gatewayContext, cancellationToken);
                        //serviceResult = await service.InvokeMethodHandler(gatewayContext, message.m_targetUri, ReadOnlySpan<byte>.Empty, message, cancellationToken);
                        success = true;
                    } catch (Exception e)
                    {
                        if (!settings.m_swallowExceptions)
                        {
                            throw;
                        }
                        
                        logger.LogError(e, "Error in AMF service handler. Target Uri: {TargetUri}", message.m_targetUri);
                        serviceResult = null;
                        success = false;
                    }
                    
                    var targetUri = success ? 
                        $"{message.m_responseUri}/onResult" :
                        $"{message.m_responseUri}/onStatus";
                    responsePacket.m_messages.Add(new AmfMessage
                    {
                        m_targetUri = targetUri,
                        m_responseUri = "null",
                        m_data = serviceResult,
                    });
                }
                
                var serialized = AmfPolyType.Serialize(responsePacket, settings.m_options, settings.m_shapeProvider);
                return Results.Bytes(serialized, CONTENT_TYPE);
            }).Accepts(null!, CONTENT_TYPE);
        }
    }
}