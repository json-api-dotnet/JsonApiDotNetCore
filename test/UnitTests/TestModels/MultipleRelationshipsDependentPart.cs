using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace UnitTests.TestModels
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class MultipleRelationshipsDependentPart : IdentifiableWithAttribute
    {
        [HasOne]
        public OneToOnePrincipal PopulatedToOne { get; set; }

        public int PopulatedToOneId { get; set; }

        [HasOne]
        public OneToOnePrincipal EmptyToOne { get; set; }

        public int? EmptyToOneId { get; set; }

        [HasOne]
        public OneToManyPrincipal PopulatedToMany { get; set; }

        public int PopulatedToManyId { get; set; }

        [HasOne]
        public OneToManyPrincipal EmptyToMany { get; set; }

        public int? EmptyToManyId { get; set; }
    }
}
