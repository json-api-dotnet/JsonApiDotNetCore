using JetBrains.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Microservices.Messages
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class UserMovedToGroupContent : IMessageContent
    {
        public int FormatVersion => 1;

        public Guid UserId { get; }
        public Guid BeforeGroupId { get; }
        public Guid AfterGroupId { get; }

        public UserMovedToGroupContent(Guid userId, Guid beforeGroupId, Guid afterGroupId)
        {
            UserId = userId;
            BeforeGroupId = beforeGroupId;
            AfterGroupId = afterGroupId;
        }
    }
}
