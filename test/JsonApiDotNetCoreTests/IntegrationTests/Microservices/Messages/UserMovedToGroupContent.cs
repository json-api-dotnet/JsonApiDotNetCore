#nullable disable

using System;
using JetBrains.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Microservices.Messages
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class UserMovedToGroupContent : IMessageContent
    {
        public int FormatVersion => 1;

        public Guid UserId { get; set; }
        public Guid BeforeGroupId { get; set; }
        public Guid AfterGroupId { get; set; }
    }
}
