using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Errors
{
    /// <summary>
    /// The error that is thrown when assigning one or more non-existing resources to a relationship.
    /// </summary>
    public sealed class ResourcesInRelationshipAssignmentsNotFoundException : Exception
    {
        public IReadOnlyCollection<Error> Errors { get; }

        public ResourcesInRelationshipAssignmentsNotFoundException(IEnumerable<MissingResourceInRelationship> missingResources)
        {
            Errors = missingResources.Select(CreateError).ToList();
        }

        private Error CreateError(MissingResourceInRelationship missingResourceInRelationship)
        {
            return new Error(HttpStatusCode.NotFound)
            {
                Title = "A resource being assigned to a relationship does not exist.",
                Detail =
                    $"Resource of type '{missingResourceInRelationship.ResourceType}' with ID '{missingResourceInRelationship.ResourceId}' " +
                    $"being assigned to relationship '{missingResourceInRelationship.RelationshipName}' does not exist."
            };
        }
    }
}
