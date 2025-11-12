using BlogAngular.Application.Common.Models;
using BlogAngular.Web.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace BlogAngular.Web.Infrastructure
{
    public abstract class EndpointGroupBase : ControllerBase
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
            => this.Mediator.Send(request).ToActionResult();

        protected Task<ActionResult<TResult>> Send<TResult>(
            IRequest<Result<TResult>> request)
            => this.Mediator.Send(request).ToActionResult();

        protected Task<ActionResult> Send(
            IRequest<Result> request)
            => this.Mediator.Send(request).ToActionResult();

        public virtual string? GroupName { get; }
        public abstract void Map(RouteGroupBuilder groupBuilder);
    }
}
