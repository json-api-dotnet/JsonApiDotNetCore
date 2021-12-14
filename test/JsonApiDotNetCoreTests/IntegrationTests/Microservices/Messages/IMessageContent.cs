namespace JsonApiDotNetCoreTests.IntegrationTests.Microservices.Messages;

public interface IMessageContent
{
    // Increment when content structure changes.
    int FormatVersion { get; }
}
