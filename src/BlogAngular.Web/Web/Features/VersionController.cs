using System.Threading.Tasks;
using BlogAngular.Application.Common.Version;
using Microsoft.AspNetCore.Mvc;

namespace BlogAngular.Web.Features
{
    public class VersionController : ApiController
    {
        [HttpGet]
        public async Task<ActionResult<VersionResponseEnvelope>> Index()
           => await this.Send(new VersionCommand());
    }

}