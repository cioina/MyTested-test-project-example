namespace BlogAngular.Test.Routing;
#if DEBUG

using Application.Common.Version;
using MyTested.AspNetCore.Mvc;
using Web.Features;
using Xunit;

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
#endif



