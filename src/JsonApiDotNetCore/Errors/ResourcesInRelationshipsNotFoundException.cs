using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Errors
{
    /// <summary>
    /// The error that is thrown when referencing one or more non-existing resources in one or more relationships.
    /// </summary>
    public sealed class ResourcesInRelationshipsNotFoundException : Exception, IHasMultipleErrors
    {
        public IReadOnlyCollection<Error> Errors { get; }

        public ResourcesInRelationshipsNotFoundException(IEnumerable<MissingResourceInRelationship> missingResources)
        {
            Errors = missingResources.Select(CreateError).ToList();
        }

        private Error CreateError(MissingResourceInRelationship missingResourceInRelationship)
        {
            return new Error(HttpStatusCode.NotFound)
            {
                Title = "A related resource does not exist.",
                Detail =
                    $"Related resource of type '{missingResourceInRelationship.ResourceType}' with ID '{missingResourceInRelationship.ResourceId}' " +
                    $"in relationship '{missingResourceInRelationship.RelationshipName}' does not exist."
            };
        }
    }
}
