#if DEBUG
using BlogAngular.Application.Common.Version;
using BlogAngular.Web.Features;
using MyTested.AspNetCore.Mvc;
using Xunit;

namespace BlogAngular.Test.Routing
{
    public class FrontEndRouteTest
    {
        [Fact]
        public void VersionShouldBeRouted()
            => MyMvc
            .Pipeline()
            .ShouldMap(request => request
                .WithMethod(HttpMethod.Get)
                .WithLocation("api/v1.0/version"))
            .To<VersionController>(c => c.Index())
            .Which()
            .ShouldReturn()
            .ActionResult(result => result.Result(new VersionResponseEnvelope
            {
                VersionJson = new VersionResponseModel()
            }));
    }
}
#endif



