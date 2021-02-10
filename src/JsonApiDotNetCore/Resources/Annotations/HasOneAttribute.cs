using System;

namespace JsonApiDotNetCore.Resources.Annotations
{
    /// <summary>
    /// Used to expose a property on a resource class as a JSON:API to-one relationship (https://jsonapi.org/format/#document-resource-object-relationships).
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class HasOneAttribute : RelationshipAttribute
    {
    }
}
