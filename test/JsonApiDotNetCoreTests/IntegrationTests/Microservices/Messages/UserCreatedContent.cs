using JetBrains.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Microservices.Messages;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class UserCreatedContent(Guid userId, string userLoginName, string? userDisplayName) : IMessageContent
{
    public int FormatVersion => 1;

    public Guid UserId { get; } = userId;
    public string UserLoginName { get; } = userLoginName;
    public string? UserDisplayName { get; } = userDisplayName;
}
