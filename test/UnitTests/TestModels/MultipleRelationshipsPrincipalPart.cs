using System.Collections.Generic;
using JsonApiDotNetCore.Resources.Annotations;

namespace UnitTests.TestModels
{
    public sealed class MultipleRelationshipsPrincipalPart : IdentifiableWithAttribute
    {
        [HasOne]
        public OneToOneDependent PopulatedToOne { get; set; }

        [HasOne]
        public OneToOneDependent EmptyToOne { get; set; }

        [HasMany]
        public ISet<OneToManyDependent> PopulatedToManies { get; set; }

        [HasMany]
        public ISet<OneToManyDependent> EmptyToManies { get; set; }

        [HasOne]
        public MultipleRelationshipsPrincipalPart Multi { get; set; }
    }
}
