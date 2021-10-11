#nullable disable

using System;
using JetBrains.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Microservices.Messages
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class UserLoginNameChangedContent : IMessageContent
    {
        public int FormatVersion => 1;

        public Guid UserId { get; set; }
        public string BeforeUserLoginName { get; set; }
        public string AfterUserLoginName { get; set; }
    }
}
