using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Serialization;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
// The constructor parameters will be resolved from the container, which means you can take on any dependency that is also defined in the container.
public sealed class StudentDefinition(IResourceGraph resourceGraph, IEncryptionService encryptionService, ResourceDefinitionHitCounter hitCounter)
    : HitCountingResourceDefinition<Student, long>(resourceGraph, hitCounter)
{
    private readonly IEncryptionService _encryptionService = encryptionService;

    protected override ResourceDefinitionExtensibilityPoints ExtensibilityPointsToTrack => ResourceDefinitionExtensibilityPoints.Serialization;

    public override void OnDeserialize(Student resource)
    {
        base.OnDeserialize(resource);

        if (!string.IsNullOrEmpty(resource.SocialSecurityNumber))
        {
            resource.SocialSecurityNumber = _encryptionService.Decrypt(resource.SocialSecurityNumber);
        }
    }

    public override void OnSerialize(Student resource)
    {
        base.OnSerialize(resource);

        if (!string.IsNullOrEmpty(resource.SocialSecurityNumber))
        {
            resource.SocialSecurityNumber = _encryptionService.Encrypt(resource.SocialSecurityNumber);
        }
    }
}
