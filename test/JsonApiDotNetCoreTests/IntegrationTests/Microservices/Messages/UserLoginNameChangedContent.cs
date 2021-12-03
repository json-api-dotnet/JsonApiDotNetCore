using JetBrains.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Microservices.Messages
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class UserLoginNameChangedContent : IMessageContent
    {
        public int FormatVersion => 1;

        public Guid UserId { get; }
        public string BeforeUserLoginName { get; }
        public string AfterUserLoginName { get; }

        public UserLoginNameChangedContent(Guid userId, string beforeUserLoginName, string afterUserLoginName)
        {
            UserId = userId;
            BeforeUserLoginName = beforeUserLoginName;
            AfterUserLoginName = afterUserLoginName;
        }
    }
}
