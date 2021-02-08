using JsonApiDotNetCore.Resources;

namespace TestBuildingBlocks
{
    /// <summary>
    /// Ensures the resource attributes are returned when creating/updating a resource.
    /// </summary>
    public sealed class NeverSameResourceChangeTracker<TResource> : IResourceChangeTracker<TResource>
        where TResource : class, IIdentifiable
    {
        public void SetInitiallyStoredAttributeValues(TResource resource)
        {
        }

        public void SetRequestedAttributeValues(TResource resource)
        {
        }

        public void SetFinallyStoredAttributeValues(TResource resource)
        {
        }

        public bool HasImplicitChanges()
        {
            return true;
        }
    }
}
