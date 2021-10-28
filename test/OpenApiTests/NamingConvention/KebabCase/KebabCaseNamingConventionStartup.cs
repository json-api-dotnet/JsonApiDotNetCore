using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using Microsoft.EntityFrameworkCore;
using OpenApiTests.LegacyOpenApiIntegration;

namespace OpenApiTests.NamingConvention.KebabCase
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class KebabCaseNamingConventionStartup<TDbContext> : OpenApiStartup<TDbContext>
        where TDbContext : DbContext
    {
        protected override void SetJsonApiOptions(JsonApiOptions options)
        {
            base.SetJsonApiOptions(options);

            options.SerializerOptions.PropertyNamingPolicy = JsonKebabCaseNamingPolicy.Instance;
            options.SerializerOptions.DictionaryKeyPolicy = JsonKebabCaseNamingPolicy.Instance;
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        }
    }
}
