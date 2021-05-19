using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.AtomicOperations.ResourceDefinitions.Serialization
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class RecordCompanyDefinition : JsonApiResourceDefinition<RecordCompany, short>
    {
        private readonly AtomicSerializationHitCounter _hitCounter;

        public RecordCompanyDefinition(IResourceGraph resourceGraph, AtomicSerializationHitCounter hitCounter)
            : base(resourceGraph)
        {
            _hitCounter = hitCounter;
        }

        public override void OnDeserialize(RecordCompany resource)
        {
            _hitCounter.IncrementDeserializeCount();

            if (!string.IsNullOrEmpty(resource.Name))
            {
                resource.Name = resource.Name.ToUpperInvariant();
            }
        }

        public override void OnSerialize(RecordCompany resource)
        {
            _hitCounter.IncrementSerializeCount();

            if (!string.IsNullOrEmpty(resource.CountryOfResidence))
            {
                resource.CountryOfResidence = resource.CountryOfResidence.ToUpperInvariant();
            }
        }
    }
}
