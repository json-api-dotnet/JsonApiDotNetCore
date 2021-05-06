using System;
using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Issue988
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public abstract class EntityBase<TId> : Identifiable<TId>
    {
        public DateTimeOffset? DateCreated { get; set; }

        public DateTimeOffset? DateModified { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}
