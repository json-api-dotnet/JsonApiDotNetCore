using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BackgroundWorkerService.Interop;

internal sealed class FakeLinkGenerator : LinkGenerator
{
    public override string GetPathByAddress<TAddress>(HttpContext httpContext, TAddress address, RouteValueDictionary values,
        RouteValueDictionary? ambientValues = null, PathString? pathBase = null, FragmentString fragment = new(), LinkOptions? options = null)
    {
        throw new NotImplementedException();
    }

    public override string GetPathByAddress<TAddress>(TAddress address, RouteValueDictionary values, PathString pathBase = new(),
        FragmentString fragment = new(), LinkOptions? options = null)
    {
        throw new NotImplementedException();
    }

    public override string GetUriByAddress<TAddress>(HttpContext httpContext, TAddress address, RouteValueDictionary values,
        RouteValueDictionary? ambientValues = null, string? scheme = null, HostString? host = null, PathString? pathBase = null,
        FragmentString fragment = new(), LinkOptions? options = null)
    {
        throw new NotImplementedException();
    }

    public override string GetUriByAddress<TAddress>(TAddress address, RouteValueDictionary values, string? scheme, HostString host,
        PathString pathBase = new(), FragmentString fragment = new(), LinkOptions? options = null)
    {
        throw new NotImplementedException();
    }
}
