using JetBrains.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Microservices.Messages;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class GroupCreatedContent : IMessageContent
{
    public int FormatVersion => 1;

    public Guid GroupId { get; }
    public string GroupName { get; }

    public GroupCreatedContent(Guid groupId, string groupName)
    {
        GroupId = groupId;
        GroupName = groupName;
    }
}
