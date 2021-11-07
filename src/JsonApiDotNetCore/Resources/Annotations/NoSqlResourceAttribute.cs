using System;
using System.Reflection;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCore.Resources.Annotations
{
    /// <summary>
    /// When put on a resource class, marks that resource as being hosted in a NoSQL database.
    /// </summary>
    /// <seealso cref="NoSqlServiceCollectionExtensions.AddNoSqlResourceServices(IServiceCollection)" />
    /// <seealso cref="NoSqlServiceCollectionExtensions.AddNoSqlResourceServices(IServiceCollection, Assembly)" />
    [PublicAPI]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
    public class NoSqlResourceAttribute : Attribute
    {
    }
}
