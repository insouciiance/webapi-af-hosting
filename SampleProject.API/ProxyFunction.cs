using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;

namespace SampleProject.API;

public class ProxyFunction(IApplicationBuilder app, RequestDelegate handler)
{
    [Function("ProxyFunction")]
    public async Task Run(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "put", "patch", "delete", "options", Route = "{*any}")] HttpRequestData req)
    {
        var ctx = req.FunctionContext.GetHttpContext()!;

        using var scope = app.ApplicationServices.CreateScope();

        ctx.Features.Get<IEndpointFeature>()!.Endpoint = null;
        ctx.RequestServices = scope.ServiceProvider;

        if (ctx.RequestServices.GetService<IHttpContextAccessor>() is { } accessor)
        {
            accessor.HttpContext = ctx;
        }

        await handler.Invoke(ctx);
    }
}
