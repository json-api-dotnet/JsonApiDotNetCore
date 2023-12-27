using JetBrains.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Microservices.Messages;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class GroupRenamedContent(Guid groupId, string beforeGroupName, string afterGroupName) : IMessageContent
{
    public int FormatVersion => 1;

    public Guid GroupId { get; } = groupId;
    public string BeforeGroupName { get; } = beforeGroupName;
    public string AfterGroupName { get; } = afterGroupName;
}
