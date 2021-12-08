using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using Microsoft.EntityFrameworkCore;

namespace OpenApiTests.NamingConvention.CamelCase;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class CamelCaseNamingConventionStartup<TDbContext> : OpenApiStartup<TDbContext>
    where TDbContext : DbContext
{
    protected override void SetJsonApiOptions(JsonApiOptions options)
    {
        base.SetJsonApiOptions(options);

        options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.SerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    }
}
