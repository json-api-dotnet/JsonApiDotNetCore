#nullable disable

using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Serialization
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class StudentDefinition : JsonApiResourceDefinition<Student, int>
    {
        private readonly IEncryptionService _encryptionService;
        private readonly ResourceDefinitionHitCounter _hitCounter;

        public StudentDefinition(IResourceGraph resourceGraph, IEncryptionService encryptionService, ResourceDefinitionHitCounter hitCounter)
            : base(resourceGraph)
        {
            // This constructor will be resolved from the container, which means
            // you can take on any dependency that is also defined in the container.

            _encryptionService = encryptionService;
            _hitCounter = hitCounter;
        }

        public override void OnDeserialize(Student resource)
        {
            _hitCounter.TrackInvocation<Student>(ResourceDefinitionHitCounter.ExtensibilityPoint.OnDeserialize);

            if (!string.IsNullOrEmpty(resource.SocialSecurityNumber))
            {
                resource.SocialSecurityNumber = _encryptionService.Decrypt(resource.SocialSecurityNumber);
            }
        }

        public override void OnSerialize(Student resource)
        {
            _hitCounter.TrackInvocation<Student>(ResourceDefinitionHitCounter.ExtensibilityPoint.OnSerialize);

            if (!string.IsNullOrEmpty(resource.SocialSecurityNumber))
            {
                resource.SocialSecurityNumber = _encryptionService.Encrypt(resource.SocialSecurityNumber);
            }
        }
    }
}
