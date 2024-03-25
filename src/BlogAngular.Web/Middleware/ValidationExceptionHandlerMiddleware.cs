namespace BlogAngular.Web.Middleware;

using Application.Common.Exceptions;
using Application.Common.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Net;
using System.Threading.Tasks;

public class ValidationExceptionHandlerMiddleware
{
    private readonly RequestDelegate next;

    public ValidationExceptionHandlerMiddleware(RequestDelegate next)
        => this.next = next;

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await this.next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var code = HttpStatusCode.InternalServerError;

        var result = string.Empty;

        switch (exception)
        {
            case ModelValidationException modelValidationException:
                code = HttpStatusCode.UnprocessableEntity;
                result = SerializeObject(
                    new
                    {
                        modelValidationException.Errors
                    }
                );
                break;
        }

        if (string.IsNullOrEmpty(result))
        {
            var error = exception.Message;

            if (exception is Ardalis.GuardClauses.NotFoundException or ArgumentException)
            {
                code = HttpStatusCode.UnprocessableEntity;
            }
            else if (exception.Source == "Microsoft.EntityFrameworkCore.Relational" &&
                error == "An error occurred while saving the entity changes. See the inner exception for details.")
            {
                error = exception.InnerException!.Message;
                code = HttpStatusCode.UnprocessableEntity;
            }

            result = SerializeObject(
                new ErrorListResult(exception.GetType().Name, new[] { error })
                );
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        return context.Response.WriteAsync(result);
    }

    private static string SerializeObject(object obj)
        => JsonConvert.SerializeObject(obj, new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy(true, true)
            }
        });
}

public static class ValidationExceptionHandlerMiddlewareExtensions
{
    public static IApplicationBuilder UseValidationExceptionHandler(
        this IApplicationBuilder builder)
        => builder.UseMiddleware<ValidationExceptionHandlerMiddleware>();
}