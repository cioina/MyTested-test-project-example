namespace BlogAngular.Web;

using Application.Common.Contracts;
using Application.Common.Models;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Services;

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