using JetBrains.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Microservices.Messages
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class UserDeletedContent : IMessageContent
    {
        public int FormatVersion => 1;

        public Guid UserId { get; }

        public UserDeletedContent(Guid userId)
        {
            UserId = userId;
        }
    }
}
