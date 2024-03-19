namespace BlogAngular.Web.Features;

using Application.Common.Version;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

public class VersionController : ApiController
{
    [HttpGet]
    public async Task<ActionResult<VersionResponseEnvelope>> Index()
       => await this.Send(new VersionCommand());
}