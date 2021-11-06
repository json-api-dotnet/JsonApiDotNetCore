using System;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.Resources.Annotations
{
    /// <summary>
    /// When put on a resource class, marks that resource as being hosted in a NoSQL database.
    /// The <see cref="NoSqlServiceCollectionExtensions.AddNoSqlResourceServices" /> will
    /// register a <see cref="NoSqlResourceService{TResource,TId}" /> for each resource
    /// having this attribute.
    /// </summary>
    [PublicAPI]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
    public class NoSqlResourceAttribute : Attribute
    {
    }
}
