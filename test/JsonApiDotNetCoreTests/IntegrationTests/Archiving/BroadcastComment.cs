using System;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Archiving
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class BroadcastComment : Identifiable
    {
        [Attr]
        public string Text { get; set; }

        [Attr]
        public DateTimeOffset CreatedAt { get; set; }

        [HasOne]
        public TelevisionBroadcast AppliesTo { get; set; }
    }
}
