namespace BlogAngular.Web.Features;

using Application.Identity.Commands.Common;
using Application.Identity.Commands.Login;
using Application.Identity.Commands.Register;
using Application.Identity.Commands.Update;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using static Domain.Common.Models.ModelConstants.Identity;

public class IdentityController : ApiController
{
    [HttpPost]
    [Authorize(AuthenticationSchemes = Bearer, Policy = BearerPolicy, Roles = AdministratorRoleName)]
    public async Task<ActionResult<UserResponseEnvelope>> LoginPassword(
        LoginPasswordCommand command)
        => await this.Send(command);

    [HttpPost]
    [Route(nameof(Login))]
    public async Task<ActionResult<UserResponseEnvelope>> Login(
        UserLoginCommand command)
        => await this.Send(command);

    [HttpPost]
    [Route(nameof(Register))]
    public async Task<ActionResult<UserResponseEnvelope>> Register(
        UserRegisterCommand command)
        => await this.Send(command);

    [HttpPut]
    [Route(nameof(Update))]
    [Authorize(AuthenticationSchemes = Bearer, Policy = BearerPolicy, Roles = AdministratorRoleName)]
    public async Task<ActionResult<UserResponseEnvelope>> Update(
        UserUpdateCommand command)
        => await this.Send(command);

}