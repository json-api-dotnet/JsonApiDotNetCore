using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.AtomicOperations;

public interface IVersionTracker
{
    bool RequiresVersionTracking();

    void CaptureVersions(ResourceType resourceType, IIdentifiable resource);

    string? GetVersion(ResourceType resourceType, string stringId);
}
