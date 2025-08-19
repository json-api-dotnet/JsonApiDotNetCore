using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using TestBuildingBlocks;

namespace OpenApiTests.Documentation;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class DocumentationStartup<TDbContext> : OpenApiStartup<TDbContext>
    where TDbContext : TestableDbContext
{
    protected override void ConfigureJsonApiOptions(JsonApiOptions options)
    {
        base.ConfigureJsonApiOptions(options);

        options.ClientIdGeneration = ClientIdGenerationMode.Allowed;
    }

    protected override void ConfigureSwaggerGenOptions(SwaggerGenOptions options)
    {
        base.ConfigureSwaggerGenOptions(options);

        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = "v1",
            Title = "Skyscrapers of the world",
            Description = "A JSON:API service for managing skyscrapers.",
            Contact = new OpenApiContact
            {
                Name = "Report issues",
                Url = new Uri("https://github.com/json-api-dotnet/JsonApiDotNetCore/issues")
            },
            License = new OpenApiLicense
            {
                Name = "MIT License",
                Url = new Uri("https://licenses.nuget.org/MIT")
            }
        });
    }
}
