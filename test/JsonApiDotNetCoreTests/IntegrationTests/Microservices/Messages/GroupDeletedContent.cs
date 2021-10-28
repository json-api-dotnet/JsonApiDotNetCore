using System;
using JetBrains.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Microservices.Messages
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class GroupDeletedContent : IMessageContent
    {
        public int FormatVersion => 1;

        public Guid GroupId { get; }

        public GroupDeletedContent(Guid groupId)
        {
            GroupId = groupId;
        }
    }
}
