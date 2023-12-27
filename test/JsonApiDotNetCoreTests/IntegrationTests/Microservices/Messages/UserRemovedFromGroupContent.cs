using JetBrains.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Microservices.Messages;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class UserRemovedFromGroupContent(Guid userId, Guid groupId) : IMessageContent
{
    public int FormatVersion => 1;

    public Guid UserId { get; } = userId;
    public Guid GroupId { get; } = groupId;
}
