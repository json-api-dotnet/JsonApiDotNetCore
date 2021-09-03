using System;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.QueryStrings
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Appointment : Identifiable
    {
        [Attr]
        public string Title { get; set; }

        [Attr]
        public DateTimeOffset StartTime { get; set; }

        [Attr]
        public DateTimeOffset EndTime { get; set; }
    }
}
