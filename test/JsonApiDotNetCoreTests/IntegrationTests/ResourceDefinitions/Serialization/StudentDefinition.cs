using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Serialization
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class StudentDefinition : HitCountingResourceDefinition<Student, int>
    {
        private readonly IEncryptionService _encryptionService;

        protected override ResourceDefinitionExtensibilityPoints ExtensibilityPointsToTrack => ResourceDefinitionExtensibilityPoints.Serialization;

        public StudentDefinition(IResourceGraph resourceGraph, IEncryptionService encryptionService, ResourceDefinitionHitCounter hitCounter)
            : base(resourceGraph, hitCounter)
        {
            // This constructor will be resolved from the container, which means
            // you can take on any dependency that is also defined in the container.

            _encryptionService = encryptionService;
        }

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
}
