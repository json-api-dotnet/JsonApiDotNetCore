using JetBrains.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Microservices.Messages;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class UserMovedToGroupContent(Guid userId, Guid beforeGroupId, Guid afterGroupId) : IMessageContent
{
    public int FormatVersion => 1;

    public Guid UserId { get; } = userId;
    public Guid BeforeGroupId { get; } = beforeGroupId;
    public Guid AfterGroupId { get; } = afterGroupId;
}
