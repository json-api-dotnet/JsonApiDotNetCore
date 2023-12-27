using JetBrains.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Microservices.Messages;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class UserDisplayNameChangedContent(Guid userId, string? beforeUserDisplayName, string? afterUserDisplayName) : IMessageContent
{
    public int FormatVersion => 1;

    public Guid UserId { get; } = userId;
    public string? BeforeUserDisplayName { get; } = beforeUserDisplayName;
    public string? AfterUserDisplayName { get; } = afterUserDisplayName;
}
