using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using TestBuildingBlocks;

namespace OpenApiTests.DocComments;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class DocCommentsStartup<TDbContext> : OpenApiStartup<TDbContext>
    where TDbContext : TestableDbContext
{
    protected override void SetupSwaggerGenAction(SwaggerGenOptions options)
    {
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

        base.SetupSwaggerGenAction(options);
    }
}
