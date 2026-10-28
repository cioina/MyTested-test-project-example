using Asp.Versioning;
using BlogAngular.Application.Common.Models;
using BlogAngular.Web.Extensions;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace BlogAngular.Web
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public abstract class ApiController : ControllerBase
    {
        protected const string Id = "{id}";
        protected const string PathSeparator = "/";

        private IMediator? mediator;

        protected IMediator Mediator
            => this.mediator ??= this.HttpContext
                .RequestServices
                .GetRequiredService<IMediator>();

        protected Task<ActionResult<TResult>> Send<TResult>(
            IRequest<TResult> request)
            => Mediator.Send(request).AsTask().ToActionResult();

        protected Task<ActionResult<TResult>> Send<TResult>(
            IRequest<Result<TResult>> request)
            => Mediator.Send(request).AsTask().ToActionResult();

        protected Task<ActionResult> Send(
            IRequest<Result> request)
            => Mediator.Send(request).AsTask().ToActionResult();

        static protected Task<ActionResult<TResult>> Send<TResult>(IMediator sender,
            IRequest<Result<TResult>> request)
            => sender.Send(request).AsTask().ToActionResult();
    }
}