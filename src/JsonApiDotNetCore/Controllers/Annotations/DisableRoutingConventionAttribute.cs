using System;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.Controllers.Annotations
{
    /// <summary>
    /// Used on an ASP.NET Core controller class to indicate that a custom route is used instead of the built-in routing convention.
    /// </summary>
    /// <example><![CDATA[
    /// [DisableRoutingConvention, Route("some/custom/route/to/customers")]
    /// public class CustomersController : JsonApiController<Customer> { }
    /// ]]></example>
    [PublicAPI]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
    public sealed class DisableRoutingConventionAttribute : Attribute
    {
    }
}
