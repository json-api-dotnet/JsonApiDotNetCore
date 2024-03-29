using JetBrains.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Microservices.Messages;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class UserDeletedContent(Guid userId) : IMessageContent
{
    public int FormatVersion => 1;

    public Guid UserId { get; } = userId;
}
