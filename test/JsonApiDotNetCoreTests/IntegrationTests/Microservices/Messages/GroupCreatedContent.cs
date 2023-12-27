using JetBrains.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Microservices.Messages;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class GroupCreatedContent(Guid groupId, string groupName) : IMessageContent
{
    public int FormatVersion => 1;

    public Guid GroupId { get; } = groupId;
    public string GroupName { get; } = groupName;
}
