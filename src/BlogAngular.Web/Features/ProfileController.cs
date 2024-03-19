namespace BlogAngular.Web.Features;

using Application.Identity.Queries.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using static Domain.Common.Models.ModelConstants.Identity;

public class ProfileController : ApiController
{
    [HttpGet]
    [Route(Id)]
    [Authorize(AuthenticationSchemes = Bearer, Policy = BearerPolicy, Roles = AdministratorRoleName)]
    public async Task<ActionResult<ProfileResponseEnvelope>> Index(
        [FromRoute] string id)
        => await this.Send(new ProfileQuery { UserName = id });
}