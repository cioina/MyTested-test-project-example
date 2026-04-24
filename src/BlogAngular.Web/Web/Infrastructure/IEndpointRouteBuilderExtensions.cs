using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using System;
using System.Diagnostics.CodeAnalysis;

namespace BlogAngular.Web.Infrastructure
{

    public static class EndpointRouteBuilderExtensions
    {
        public static RouteHandlerBuilder MapGetIndex(this IEndpointRouteBuilder builder, Delegate handler)
        {
            return builder.MapGet("", handler);
            // .RequireCors(options => options
            //     .WithHeaders(
            //         "Content-Type",
            //         "X-Auth-Token",
            //         "Origin",
            //         "Authorization")
            //     .WithMethods(
            //         "POST",
            //         "GET",
            //         "OPTIONS",
            //         "PUT",
            //         "PATCH",
            //         "DELETE")
            //     .WithExposedHeaders(
            //         "Cache-Control",
            //         "Content-Language",
            //         "Content-Type",
            //         "Expires",
            //         "Last-Modified",
            //         "Pragma")
            //     .AllowAnyOrigin()
            //);
        }

        public static RouteHandlerBuilder MapGet(this IEndpointRouteBuilder builder, Delegate handler, [StringSyntax("Route")] string pattern = "")
        {
            Guard.Against.AnonymousMethod(handler);

            return builder.MapGet(pattern, handler)
                  .WithName(handler.Method.Name);
        }

        public static RouteHandlerBuilder MapPost(this IEndpointRouteBuilder builder, Delegate handler, [StringSyntax("Route")] string pattern = "")
        {
            Guard.Against.AnonymousMethod(handler);

            return builder.MapPost(pattern, handler)
                .WithName(handler.Method.Name);
        }

        public static RouteHandlerBuilder MapPut(this IEndpointRouteBuilder builder, Delegate handler, [StringSyntax("Route")] string pattern)
        {
            Guard.Against.AnonymousMethod(handler);

            return builder.MapPut(pattern, handler)
                .WithName(handler.Method.Name);
        }

        public static RouteHandlerBuilder MapDelete(this IEndpointRouteBuilder builder, Delegate handler, [StringSyntax("Route")] string pattern)
        {
            Guard.Against.AnonymousMethod(handler);

            return builder.MapDelete(pattern, handler)
                .WithName(handler.Method.Name);
        }
    }
}
