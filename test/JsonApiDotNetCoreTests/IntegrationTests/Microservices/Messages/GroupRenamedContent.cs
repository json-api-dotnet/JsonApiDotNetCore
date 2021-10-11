#nullable disable

using System;
using JetBrains.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Microservices.Messages
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class GroupRenamedContent : IMessageContent
    {
        public int FormatVersion => 1;

        public Guid GroupId { get; set; }
        public string BeforeGroupName { get; set; }
        public string AfterGroupName { get; set; }
    }
}
