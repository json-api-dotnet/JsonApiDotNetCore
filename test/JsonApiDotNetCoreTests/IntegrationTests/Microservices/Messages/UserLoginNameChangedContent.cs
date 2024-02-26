using JetBrains.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Microservices.Messages;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class UserLoginNameChangedContent(Guid userId, string beforeUserLoginName, string afterUserLoginName) : IMessageContent
{
    public int FormatVersion => 1;

    public Guid UserId { get; } = userId;
    public string BeforeUserLoginName { get; } = beforeUserLoginName;
    public string AfterUserLoginName { get; } = afterUserLoginName;
}
