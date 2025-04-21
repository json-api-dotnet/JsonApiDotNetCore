using JetBrains.Annotations;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.OpenApi.Client.NSwag;

// Referenced from liquid template, to ensure the built-in JsonInheritanceConverter from NSwag is never used.
[PublicAPI]
public abstract class BlockedJsonInheritanceConverter : JsonConverter
{
    private const string DefaultDiscriminatorName = "discriminator";

    public string DiscriminatorName { get; }

    public override bool CanWrite => true;
    public override bool CanRead => true;

    protected BlockedJsonInheritanceConverter()
        : this(DefaultDiscriminatorName)
    {
    }

    protected BlockedJsonInheritanceConverter(string discriminatorName)
    {
        ArgumentException.ThrowIfNullOrEmpty(discriminatorName);

        DiscriminatorName = discriminatorName;
    }

    public override bool CanConvert(Type objectType)
    {
        return true;
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        throw new InvalidOperationException("JsonInheritanceConverter is incompatible with JSON:API and must not be used.");
    }

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        throw new InvalidOperationException("JsonInheritanceConverter is incompatible with JSON:API and must not be used.");
    }
}
