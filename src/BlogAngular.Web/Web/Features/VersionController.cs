using BlogAngular.Application.Common.Version;
//using BlogAngular.Web.Extensions;
//using BlogAngular.Web.Infrastructure;
//using MediatR;
//using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Routing;
using System.Threading.Tasks;

namespace BlogAngular.Web.Features
{
    //public class VersionController : EndpointGroupBase
    //{
    //    public override string GroupName => "version";
    //    public override void Map(RouteGroupBuilder groupBuilder)
    //    {
    //        groupBuilder.MapGetIndex(Index).AllowAnonymous();
    //    }
    //    public async Task<IActionResult> Index([FromServices] IMediator sender)
    //    {
    //        var result = await sender.Send(new VersionCommand()).ToActionResult();
    //        return Ok(result.Value);
    //    }
    //}

    //public class VersionController : ApiController
    //{
    //    [HttpGet]
    //    public async Task<ActionResult<VersionResponseEnvelope>> Index([FromServices] IMediator sender)
    //       => await this.Send(sender, new VersionCommand());
    //}

    public class VersionController : ApiController
    {
        [HttpGet]
        public async Task<ActionResult<VersionResponseEnvelope>> Index()
           => await this.Send(new VersionCommand());
    }

}