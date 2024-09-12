using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace SampleProject.API.Extensions;

public static class HostBuilderExtensions
{
    public static IHostBuilder ConfigureProxyHost<TConfigurator>(this IHostBuilder builder)
    where TConfigurator : IWebApplicationConfigurator
    {
        RepairCurrentDirectory();

        builder.ConfigureGeneratedFunctionExecutor();

        builder.ConfigureGeneratedFunctionMetadataProvider();

        builder.ConfigureServices(services =>
        {
            var builder = WebApplication.CreateBuilder();

            TConfigurator.ConfigureBuilder(builder);

            var app = builder.Build();

            TConfigurator.ConfigureApplication(app);

            var startTask = app.StartAsync();

            RequestDelegate? handler = null;

            services.AddSingleton<IApplicationBuilder>(app);
            services.AddSingleton((RequestDelegate)Handler);

            async Task Handler(HttpContext ctx)
            {
                await startTask;
                handler ??= ((IApplicationBuilder)app).Build();
                await handler.Invoke(ctx);
            }
        });

        return builder;
    }

    private static void RepairCurrentDirectory()
    {
        // HACK HACK HACK
        // When an Azure Functions app uses standby mode (possibly a bug),
        // the current directory will be "C:\local\Temp\functions\standby"
        // (even though the app itself is in "C:\home\site\wwwroot")
        // https://github.com/Azure/azure-functions-host/pull/525/commits/218ac4c790b48f0444169d7eccec6391b2a48faf#r72824871
        var entryAssembly = Assembly.GetEntryAssembly()!;
        Environment.CurrentDirectory = Path.GetDirectoryName(entryAssembly.Location)!;
    }
}
