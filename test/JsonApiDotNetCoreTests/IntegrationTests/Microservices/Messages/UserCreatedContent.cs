using System;
using JetBrains.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Microservices.Messages
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class UserCreatedContent : IMessageContent
    {
        public int FormatVersion => 1;

        public Guid UserId { get; }
        public string UserLoginName { get; }
        public string? UserDisplayName { get; }

        public UserCreatedContent(Guid userId, string userLoginName, string? userDisplayName)
        {
            UserId = userId;
            UserLoginName = userLoginName;
            UserDisplayName = userDisplayName;
        }
    }
}
