using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Archiving
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class TelevisionBroadcast : Identifiable
    {
        [Attr]
        public string Title { get; set; }

        [Attr]
        public DateTimeOffset AiredAt { get; set; }

        [Attr]
        public DateTimeOffset? ArchivedAt { get; set; }

        [HasOne]
        public TelevisionStation AiredOn { get; set; }

        [HasMany]
        public ISet<BroadcastComment> Comments { get; set; }
    }
}
