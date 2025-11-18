using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace BlogAngular.Web.Infrastructure
{
    public abstract class EndpointGroupBase: ControllerBase
    {
        protected const string Id = "{id}";
        protected const string PathSeparator = "/";

        public virtual string? GroupName { get; }
        public abstract void Map(RouteGroupBuilder groupBuilder);
    }
}
