namespace BlogAngular.Test;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Startup;

public class TestStartup : Startup
{
    public TestStartup(IConfiguration configuration)
        : base(configuration)
    {
    }

    public void ConfigureTestServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
    }
}

