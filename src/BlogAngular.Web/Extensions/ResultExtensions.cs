namespace BlogAngular.Web.Extensions;

using Application.Common.Exceptions;
using Application.Common.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

public static class ResultExtensions
{
    public static async Task<ActionResult<TData>> ToActionResult<TData>(this Task<TData> resultTask)
    {
        var result = await resultTask;
        if (result == null)
            ThrowErrors(new Dictionary<string, string[]>
                        {
                            { "not_found_error", new[] { "Not Found Resul" } }
                        });

        return result;
    }

    public static async Task<ActionResult> ToActionResult(this Task<Result> resultTask)
    {
        var result = await resultTask;
        if (!result.Succeeded)
            ThrowErrors(result.Errors);

        return new OkResult();
    }

    public static async Task<ActionResult<TData>> ToActionResult<TData>(this Task<Result<TData>> resultTask)
    {
        var result = await resultTask;
        if (!result.Succeeded)
            ThrowErrors(result.Errors);

        return result.Data;
    }

    private static void ThrowErrors(IDictionary<string, string[]> errors)
    {
        throw new ModelValidationException
        {
            Errors = errors
        };
    }
}