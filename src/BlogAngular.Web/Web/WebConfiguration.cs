using BlogAngular.Application.Common.Contracts;
using BlogAngular.Application.Common.Models;
using BlogAngular.Web.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace BlogAngular.Web
{
    public static class WebConfiguration
    {
        public static IServiceCollection AddWebComponents(
            this IServiceCollection services)
        {
            services
                .AddScoped<ICurrentUser, CurrentUserService>()
                .AddValidatorsFromAssemblyContaining<Result>()
                .AddControllers(options => options.EnableEndpointRouting = false)
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.SuppressModelStateInvalidFilter = true;
                    options.SuppressMapClientErrors = true;

                    options.DisableImplicitFromServicesParameters = true;
                    options.SuppressConsumesConstraintForFormFileParameters = true;

                })
                .AddNewtonsoftJson();

            services.AddApiVersioning();

            return services;
        }
    }
}