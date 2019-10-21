// REF: https://github.com/aspnet/Entropy/blob/dev/samples/Mvc.CustomRoutingConvention/NameSpaceRoutingConvention.cs
// REF: https://github.com/aspnet/Mvc/issues/5691
using System;

namespace JsonApiDotNetCore.Internal
{
    public interface IControllerResourceMapping
    {
        Type GetAssociatedResource(string controllerName);
    }
}