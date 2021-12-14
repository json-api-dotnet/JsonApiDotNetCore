using JetBrains.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Microservices.Messages;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class UserRemovedFromGroupContent : IMessageContent
{
    public int FormatVersion => 1;

    public Guid UserId { get; }
    public Guid GroupId { get; }

    public UserRemovedFromGroupContent(Guid userId, Guid groupId)
    {
        UserId = userId;
        GroupId = groupId;
    }
}
