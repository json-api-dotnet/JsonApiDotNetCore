using System.Text.Json;
using JetBrains.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Microservices.Messages
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class OutgoingMessage
    {
        public long Id { get; set; }
        public string Type { get; set; }
        public int FormatVersion { get; set; }
        public string Content { get; set; }

        public T GetContentAs<T>()
            where T : IMessageContent
        {
            string namespacePrefix = typeof(IMessageContent).Namespace;
            var contentType = System.Type.GetType($"{namespacePrefix}.{Type}", true);

            return (T)JsonSerializer.Deserialize(Content, contentType);
        }

        public static OutgoingMessage CreateFromContent(IMessageContent content)
        {
            return new()
            {
                Type = content.GetType().Name,
                FormatVersion = content.FormatVersion,
                Content = JsonSerializer.Serialize(content, content.GetType())
            };
        }
    }
}
