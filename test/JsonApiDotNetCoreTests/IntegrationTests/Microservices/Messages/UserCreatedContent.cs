using System;
using JetBrains.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Microservices.Messages
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class UserCreatedContent : IMessageContent
    {
        public int FormatVersion => 1;

        public Guid UserId { get; set; }
        public string UserLoginName { get; set; }
        public string UserDisplayName { get; set; }
    }
}
