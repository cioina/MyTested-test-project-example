using BlogAngular.Application.Common.Exceptions;
using BlogAngular.Application.Common.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Net;
using System.Threading.Tasks;

namespace BlogAngular.Web.Middleware
{
    public class ValidationExceptionHandlerMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate next = next;

        public async Task InvokeAsync(HttpContext context)
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
                    //Example of exception.InnerException!.Message. Please see https://github.com/cioina/angular-test-example/blob/main/version-2/article-listing.component.spec.ts
                    //Cannot insert duplicate key row in object 'dbo.Articles' with unique index 'IX_Articles_Slug'. The duplicate key value is (dotnet-core-testing).
                    error = exception.InnerException!.Message;
                    code = HttpStatusCode.UnprocessableEntity;
                }

                result = SerializeObject(
                    new ErrorListResult(exception.GetType().Name, [error])
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
}