using System.Linq;
using System.Threading.Tasks;
using LWS_Node.Configuration;
using LWS_Node.Model.Response;
using Microsoft.AspNetCore.Http;

namespace LWS_Node.Middleware;

public class NodeAuthMiddleware
{
    private readonly RequestDelegate _requestDelegate;
    private readonly NodeConfiguration _nodeConfiguration;

    public NodeAuthMiddleware(RequestDelegate requestDelegate, NodeConfiguration nodeConfiguration)
    {
        _requestDelegate = requestDelegate;
        _nodeConfiguration = nodeConfiguration;
    }

    public async Task Invoke(HttpContext context)
    {
        var nodeAuthToken = context.Request.Headers["X-NODE-AUTH"].FirstOrDefault();
        if (nodeAuthToken != _nodeConfiguration.NodeKey)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new ErrorModel
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                RequestTraceId = context.TraceIdentifier,
                Message = "Cannot validate node token! Please check node's token again."
            });
        }
        else
        {
            await _requestDelegate(context);
        }
    }
}