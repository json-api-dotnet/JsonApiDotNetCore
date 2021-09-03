using System;
using JetBrains.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Microservices.Messages
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class UserRemovedFromGroupContent : IMessageContent
    {
        public int FormatVersion => 1;

        public Guid UserId { get; set; }
        public Guid GroupId { get; set; }
    }
}
