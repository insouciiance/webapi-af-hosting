using Microsoft.AspNetCore.Builder;

namespace SampleProject.API;

public interface IWebApplicationConfigurator
{
    static abstract void ConfigureBuilder(WebApplicationBuilder builder);

    static abstract void ConfigureApplication(WebApplication app);
}
