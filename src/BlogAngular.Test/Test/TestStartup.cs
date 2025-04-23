using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BlogAngular.Test
{
    public class TestStartup(IConfiguration configuration, IWebHostEnvironment environment) : Startup.Startup(configuration, environment)
    {
        public void ConfigureTestServices(IServiceCollection services)
        {
            base.ConfigureServices(services);
        }
    }
}