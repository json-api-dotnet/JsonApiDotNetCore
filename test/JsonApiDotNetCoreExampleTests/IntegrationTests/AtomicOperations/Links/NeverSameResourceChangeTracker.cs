using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.AtomicOperations.Links
{
    internal sealed class NeverSameResourceChangeTracker<TResource> : IResourceChangeTracker<TResource>
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
