namespace BlogAngular.Web.Features;

using Application.Blog.Tags.Commands.Common;
using Application.Blog.Tags.Commands.Create;
using Application.Blog.Tags.Commands.Delete;
using Application.Blog.Tags.Commands.Edit;
using Application.Blog.Tags.Queries.Listing;
using Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using static Domain.Common.Models.ModelConstants.Identity;

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