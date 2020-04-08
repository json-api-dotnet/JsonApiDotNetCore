using System;

namespace JsonApiDotNetCore.Controllers
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
    public sealed class DisableRoutingConventionAttribute : Attribute
    { }
}
