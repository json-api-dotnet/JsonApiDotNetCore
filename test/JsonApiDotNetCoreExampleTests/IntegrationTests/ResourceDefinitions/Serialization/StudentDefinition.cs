using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceDefinitions.Serialization
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class StudentDefinition : JsonApiResourceDefinition<Student>
    {
        private readonly IEncryptionService _encryptionService;
        private readonly SerializationHitCounter _hitCounter;

        public StudentDefinition(IResourceGraph resourceGraph, IEncryptionService encryptionService, SerializationHitCounter hitCounter)
            : base(resourceGraph)
        {
            _encryptionService = encryptionService;
            _hitCounter = hitCounter;
        }

        public override void OnDeserialize(Student resource)
        {
            _hitCounter.IncrementDeserializeCount();

            if (!string.IsNullOrEmpty(resource.SocialSecurityNumber))
            {
                resource.SocialSecurityNumber = _encryptionService.Decrypt(resource.SocialSecurityNumber);
            }
        }

        public override void OnSerialize(Student resource)
        {
            _hitCounter.IncrementSerializeCount();

            if (!string.IsNullOrEmpty(resource.SocialSecurityNumber))
            {
                resource.SocialSecurityNumber = _encryptionService.Encrypt(resource.SocialSecurityNumber);
            }
        }
    }
}
