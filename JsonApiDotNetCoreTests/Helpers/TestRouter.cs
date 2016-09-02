using System;
using System.Threading.Tasks;
using JsonApiDotNetCore.Routing;
using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCoreTests.Helpers
{
    public class TestRouter : IRouter
    {
        public bool DidHandleRoute { get; set; }

        Task<bool> IRouter.HandleJsonApiRouteAsync(HttpContext context, IServiceProvider serviceProvider)
        {
            DidHandleRoute = true;
            return Task.Run(() => true);
        }
    }
}
