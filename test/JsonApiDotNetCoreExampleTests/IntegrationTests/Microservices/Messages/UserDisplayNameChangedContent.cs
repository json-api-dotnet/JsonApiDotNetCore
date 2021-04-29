using System;
using JetBrains.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Microservices.Messages
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class UserDisplayNameChangedContent : IMessageContent
    {
        public int FormatVersion => 1;

        public Guid UserId { get; set; }
        public string BeforeUserDisplayName { get; set; }
        public string AfterUserDisplayName { get; set; }
    }
}
