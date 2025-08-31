using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCoreTests.IntegrationTests.IdCompaction;

// Tip: Add [HideResourceIdTypeInOpenApi] if you're using OpenAPI with JsonApiDotNetCore.OpenApi.Swashbuckle.
public abstract class CompactIdentifiable : Identifiable<CompactGuid>
{
    protected override string? GetStringId(CompactGuid value)
    {
        return value == CompactGuid.Empty ? null : value.ToString();
    }

    protected override CompactGuid GetTypedId(string? value)
    {
        return value == null ? CompactGuid.Empty : CompactGuid.Parse(value);
    }
}
