using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Issue988
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Engagement : EntityBase<Guid>
    {
        // Data

        [Attr]
        public string Name { get; set; }

        // Navigation Properties

        [HasMany]
        public ICollection<DocumentType> DocumentTypes { get; set; } //= new List<DocumentType>();

        [HasMany]
        [EagerLoad]
        public ICollection<EngagementParty> Parties { get; set; } //= new List<EngagementParty>();

        [HasMany]
        [NotMapped]
        public ICollection<EngagementParty> FirstParties =>
            Parties.Where(party => party.Role == ModelConstants.FirstPartyRoleName).OrderBy(party => party.ShortName).ToList();

        [HasMany]
        [NotMapped]
        public ICollection<EngagementParty> SecondParties =>
            Parties.Where(party => party.Role == ModelConstants.SecondPartyRoleName).OrderBy(party => party.ShortName).ToList();
    }
}
