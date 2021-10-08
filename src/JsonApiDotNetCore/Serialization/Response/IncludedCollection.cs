using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.Response
{
    [PublicAPI]
    public sealed class IncludedCollection
    {
        private readonly List<ResourceObject> _includes = new();
        private readonly Dictionary<IIdentifiable, int> _resourceToIncludeIndexMap = new(IdentifiableComparer.Instance);

        public IList<ResourceObject> ResourceObjects => _includes;

        public ResourceObject AddOrUpdate(IIdentifiable resource, ResourceObject resourceObject)
        {
            if (!_resourceToIncludeIndexMap.ContainsKey(resource))
            {
                _includes.Add(resourceObject);
                _resourceToIncludeIndexMap.Add(resource, _includes.Count - 1);
            }
            else
            {
                if (resourceObject.Type != null)
                {
                    int existingIndex = _resourceToIncludeIndexMap[resource];
                    ResourceObject existingVersion = _includes[existingIndex];

                    if (existingVersion != resourceObject)
                    {
                        MergeRelationships(resourceObject, existingVersion);

                        return existingVersion;
                    }
                }
            }

            return resourceObject;
        }

        private static void MergeRelationships(ResourceObject incomingVersion, ResourceObject existingVersion)
        {
            // The code below handles the case where one resource is added through different include chains with different relationships.
            // We enrich the existing resource object with the added relationships coming from the second chain, to ensure correct resource linkage.
            //
            // This is best explained using an example. Consider the next inclusion chains:
            //
            // 1. reviewer.loginAttempts
            // 2. author.preferences
            // 
            // Where the relationships `reviewer` and `author` are of the same resource type `people`. Then the next rules apply:
            //
            // A. People that were included as reviewers from inclusion chain (1) should come with their `loginAttempts` included, but not those from chain (2).
            // B. People that were included as authors from inclusion chain (2) should come with their `preferences` included, but not those from chain (1).
            // C. For a person that was included as both an reviewer and author (i.e. targeted by both chains), both `loginAttempts` and `preferences` need
            //    to be present.
            //
            // For rule (C), the related resources will be included as usual, but we need to fix resource linkage here by merging the relationship objects.
            //
            // Note that this implementation breaks the overall depth-first ordering of included objects. So solve that, we'd need to use a dependency graph
            // for included objects instead of a flat list, which may affect performance. Since the ordering is not guaranteed anyway, keeping it simple for now.

            foreach ((string relationshipName, RelationshipObject relationshipObject) in existingVersion.Relationships.EmptyIfNull())
            {
                if (!relationshipObject.Data.IsAssigned)
                {
                    SingleOrManyData<ResourceIdentifierObject> incomingRelationshipData = incomingVersion.Relationships[relationshipName].Data;

                    if (incomingRelationshipData.IsAssigned)
                    {
                        relationshipObject.Data = incomingRelationshipData;
                    }
                }
            }
        }
    }
}
