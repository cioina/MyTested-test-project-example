using BlogAngular.Application.Blog.Tags.Commands.Common;
using BlogAngular.Application.Blog.Tags.Commands.Create;
using BlogAngular.Application.Blog.Tags.Commands.Delete;
using BlogAngular.Application.Blog.Tags.Commands.Edit;
using BlogAngular.Application.Blog.Tags.Queries.Listing;
using BlogAngular.Application.Common;
//using BlogAngular.Web.Infrastructure;
using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Authorization.Infrastructure;
//using Microsoft.AspNetCore.Builder;
//using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
//using System.Security.Claims;
using System.Threading.Tasks;
using static BlogAngular.Domain.Common.Models.ModelConstants.Identity;

namespace BlogAngular.Web.Features
{
    //public class TagsController : EndpointGroupBase
    //{
    //    public override string GroupName => "tags";
    //    public override void Map(RouteGroupBuilder groupBuilder)
    //    {
    //        var authorizationPolicy = new AuthorizationPolicy([new DenyAnonymousAuthorizationRequirement(),
    //            new ClaimsAuthorizationRequirement(ClaimTypes.NameIdentifier, null),
    //            new RolesAuthorizationRequirement([AdministratorRoleName])], [Bearer]);

    //        groupBuilder.MapGetIndex(Tags);
    //        groupBuilder.MapPost(Create, nameof(Create)).RequireAuthorization([BearerPolicy]);
    //        groupBuilder.MapPut(Edit, nameof(Edit) + PathSeparator + Id).RequireAuthorization(authorizationPolicy);
    //        groupBuilder.MapDelete(Delete, nameof(Delete) + PathSeparator + Id).RequireAuthorization(authorizationPolicy);
    //    }
    //    //https://github.com/dotnet/aspnetcore/issues/55719
    //    public async Task<ActionResult<TagsResponseEnvelope>> Tags(
    //        [AsParameters] TagsQuery query)
    //        => await this.Send(query);

    //    public async Task<ActionResult<TagResponseEnvelope>> Create(
    //        [FromBody] TagCreateCommand command)
    //        => await this.Send(command);

    //    public async Task<ActionResult<TagResponseEnvelope>> Edit(
    //        [FromRoute] int id,
    //        [FromBody] TagEditCommand command)
    //        => await this.Send(command.SetId(id));

    //    public async Task<ActionResult<int>> Delete(
    //        [FromBody] TagDeleteCommand command)
    //        => await this.Send(command);
    //}

    public class TagsController : ApiController
    {
        [HttpGet]
        public async Task<ActionResult<TagsResponseEnvelope>> Tags(
            [FromQuery] TagsQuery query)
            => await this.Send(query);

        [HttpPost]
        [Route(nameof(Create))]
        [Authorize(AuthenticationSchemes = Bearer, Policy = BearerPolicy, Roles = AdministratorRoleName)]
        public async Task<ActionResult<TagResponseEnvelope>> Create(
            TagCreateCommand command)
            => await this.Send(command);

        [HttpPut]
        [Route(nameof(Edit) + PathSeparator + Id)]
        [Authorize(AuthenticationSchemes = Bearer, Policy = BearerPolicy, Roles = AdministratorRoleName)]
        public async Task<ActionResult<TagResponseEnvelope>> Edit(
            [FromRoute] int id,
            TagEditCommand command)
            => await this.Send(command.SetId(id));

        [HttpDelete]
        [Route(nameof(Delete) + PathSeparator + Id)]
        [Authorize(AuthenticationSchemes = Bearer, Policy = BearerPolicy, Roles = AdministratorRoleName)]
        public async Task<ActionResult<int>> Delete(
            [FromRoute] TagDeleteCommand command)
            => await this.Send(command);
    }
}