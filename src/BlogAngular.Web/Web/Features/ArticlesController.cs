using BlogAngular.Application.Blog.Articles.Commands.Common;
using BlogAngular.Application.Blog.Articles.Commands.Create;
using BlogAngular.Application.Blog.Articles.Commands.Delete;
using BlogAngular.Application.Blog.Articles.Commands.Details;
using BlogAngular.Application.Blog.Articles.Commands.Edit;
using BlogAngular.Application.Blog.ArticleTags.Commands.Create;
using BlogAngular.Application.Blog.ArticleTags.Commands.Listing;
using BlogAngular.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

using static BlogAngular.Domain.Common.Models.ModelConstants.Identity;

namespace BlogAngular.Web.Features
{
    public class ArticlesController : ApiController
    {
        [HttpPost]
        public async Task<ActionResult<ArticlesResponseEnvelope>> Articles(
            ArticleTagsListingCommand command)
            => await this.Send(command);

        [HttpPost]
        [Route(nameof(All))]
        [Authorize(AuthenticationSchemes = Bearer, Policy = BearerPolicy, Roles = AdministratorRoleName)]
        public async Task<ActionResult<ArticlesResponseEnvelope>> All(
            ArticleTagsListingCommand command)
            => await this.Send(command);

        [HttpPost]
        [Route(nameof(Create))]
        [Authorize(AuthenticationSchemes = Bearer, Policy = BearerPolicy, Roles = AdministratorRoleName)]
        public async Task<ActionResult<ArticleResponseEnvelope>> Create(
            ArticleCreateCommand command)
            => await this.Send(command);

        [HttpPut]
        [Route(nameof(Edit) + PathSeparator + Id)]
        [Authorize(AuthenticationSchemes = Bearer, Policy = BearerPolicy, Roles = AdministratorRoleName)]
        public async Task<ActionResult<ArticleResponseEnvelope>> Edit(
            [FromRoute] int id,
            ArticleEditCommand command)
            => await this.Send(command.SetId(id));

        [HttpGet]
        [Route(nameof(Details) + PathSeparator + Id)]
        [Authorize(AuthenticationSchemes = Bearer, Policy = BearerPolicy, Roles = AdministratorRoleName)]
        public async Task<ActionResult<ArticleResponseEnvelope>> Details(
            [FromRoute] ArticleDetailsCommand command)
            => await this.Send(command);


        [HttpDelete]
        [Route(nameof(Delete) + PathSeparator + Id)]
        [Authorize(AuthenticationSchemes = Bearer, Policy = BearerPolicy, Roles = AdministratorRoleName)]
        public async Task<ActionResult<int>> Delete(
            [FromRoute] ArticleDeleteCommand command)
            => await this.Send(command);

        [HttpPost]
        [Route(nameof(LinkTags))]
        [Authorize(AuthenticationSchemes = Bearer, Policy = BearerPolicy, Roles = AdministratorRoleName)]
        public async Task<ActionResult<int>> LinkTags(
            ArticleTagsCreateCommand command)
            => await this.Send(command);
    }
}