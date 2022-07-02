using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

#pragma warning disable AV1553 // Do not use optional parameters with default value null for strings, collections or tasks

namespace NoDbConsoleQueryExample.Interop;

/// <summary>
/// Never emits links, overcoming the limitation that the base type depends on ASP.NET.
/// </summary>
internal sealed class HiddenLinkGenerator : LinkGenerator
{
    public override string? GetPathByAddress<TAddress>(HttpContext httpContext, TAddress address, RouteValueDictionary values,
        RouteValueDictionary? ambientValues = null, PathString? pathBase = null, FragmentString fragment = new(), LinkOptions? options = null)
    {
        return null;
    }

    public override string? GetPathByAddress<TAddress>(TAddress address, RouteValueDictionary values, PathString pathBase = new(),
        FragmentString fragment = new(), LinkOptions? options = null)
    {
        return null;
    }

    public override string? GetUriByAddress<TAddress>(HttpContext httpContext, TAddress address, RouteValueDictionary values,
        RouteValueDictionary? ambientValues = null, string? scheme = null, HostString? host = null, PathString? pathBase = null,
        FragmentString fragment = new(), LinkOptions? options = null)
    {
        return null;
    }

    public override string? GetUriByAddress<TAddress>(TAddress address, RouteValueDictionary values, string? scheme, HostString host,
        PathString pathBase = new(), FragmentString fragment = new(), LinkOptions? options = null)
    {
        return null;
    }
}
