using System;
using JetBrains.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Microservices.Messages
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class UserDisplayNameChangedContent : IMessageContent
    {
        public int FormatVersion => 1;

        public Guid UserId { get; }
        public string? BeforeUserDisplayName { get; }
        public string? AfterUserDisplayName { get; }

        public UserDisplayNameChangedContent(Guid userId, string? beforeUserDisplayName, string? afterUserDisplayName)
        {
            UserId = userId;
            BeforeUserDisplayName = beforeUserDisplayName;
            AfterUserDisplayName = afterUserDisplayName;
        }
    }
}
