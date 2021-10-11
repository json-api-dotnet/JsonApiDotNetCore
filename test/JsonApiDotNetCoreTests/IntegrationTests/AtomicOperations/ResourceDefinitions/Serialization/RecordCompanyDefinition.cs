#nullable disable

using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations.ResourceDefinitions.Serialization
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class RecordCompanyDefinition : JsonApiResourceDefinition<RecordCompany, short>
    {
        private readonly ResourceDefinitionHitCounter _hitCounter;

        public RecordCompanyDefinition(IResourceGraph resourceGraph, ResourceDefinitionHitCounter hitCounter)
            : base(resourceGraph)
        {
            _hitCounter = hitCounter;
        }

        public override void OnDeserialize(RecordCompany resource)
        {
            _hitCounter.TrackInvocation<RecordCompany>(ResourceDefinitionHitCounter.ExtensibilityPoint.OnDeserialize);

            if (!string.IsNullOrEmpty(resource.Name))
            {
                resource.Name = resource.Name.ToUpperInvariant();
            }
        }

        public override void OnSerialize(RecordCompany resource)
        {
            _hitCounter.TrackInvocation<RecordCompany>(ResourceDefinitionHitCounter.ExtensibilityPoint.OnSerialize);

            if (!string.IsNullOrEmpty(resource.CountryOfResidence))
            {
                resource.CountryOfResidence = resource.CountryOfResidence.ToUpperInvariant();
            }
        }
    }
}
