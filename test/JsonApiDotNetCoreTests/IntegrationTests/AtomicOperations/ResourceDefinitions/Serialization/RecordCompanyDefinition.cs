using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations.ResourceDefinitions.Serialization
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class RecordCompanyDefinition : HitCountingResourceDefinition<RecordCompany, short>
    {
        protected override ResourceDefinitionExtensibilityPoints ExtensibilityPointsToTrack => ResourceDefinitionExtensibilityPoints.Serialization;

        public RecordCompanyDefinition(IResourceGraph resourceGraph, ResourceDefinitionHitCounter hitCounter)
            : base(resourceGraph, hitCounter)
        {
        }

        public override void OnDeserialize(RecordCompany resource)
        {
            base.OnDeserialize(resource);

            if (!string.IsNullOrEmpty(resource.Name))
            {
                resource.Name = resource.Name.ToUpperInvariant();
            }
        }

        public override void OnSerialize(RecordCompany resource)
        {
            base.OnSerialize(resource);

            if (!string.IsNullOrEmpty(resource.CountryOfResidence))
            {
                resource.CountryOfResidence = resource.CountryOfResidence.ToUpperInvariant();
            }
        }
    }
}
