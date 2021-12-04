using JetBrains.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Microservices.Messages;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class GroupRenamedContent : IMessageContent
{
    public int FormatVersion => 1;

    public Guid GroupId { get; }
    public string BeforeGroupName { get; }
    public string AfterGroupName { get; }

    public GroupRenamedContent(Guid groupId, string beforeGroupName, string afterGroupName)
    {
        GroupId = groupId;
        BeforeGroupName = beforeGroupName;
        AfterGroupName = afterGroupName;
    }
}
