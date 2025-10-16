using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.SwaggerComponents;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
internal sealed class EndpointOrderingFilter : IDocumentFilter
{
    // Workaround for docfx bug.
    private const string PatternText = @".*{id}/(?>relationships\/)?(?<RelationshipName>\w+)";
    private const RegexOptions RegexOptionsNet60 = RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture;
#pragma warning disable SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
    private static readonly Regex RelationshipNameInUrlPattern = new(PatternText, RegexOptionsNet60);
#pragma warning restore SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        ArgumentNullException.ThrowIfNull(swaggerDoc);
        ArgumentNullException.ThrowIfNull(context);

        KeyValuePair<string, IOpenApiPathItem>[] endpointsInOrder = swaggerDoc.Paths.OrderBy(GetPrimaryResourcePublicName)
            .ThenBy(GetRelationshipName).ThenBy(IsSecondaryEndpoint).ToArray();

        swaggerDoc.Paths = new OpenApiPaths();

        foreach ((string url, IOpenApiPathItem path) in endpointsInOrder)
        {
            swaggerDoc.Paths.Add(url, path);
        }
    }

    private static string GetPrimaryResourcePublicName(KeyValuePair<string, IOpenApiPathItem> entry)
    {
        if (entry.Value.Operations is { Count: > 0 })
        {
            ISet<OpenApiTagReference>? references = entry.Value.Operations.First().Value.Tags;

            if (references is { Count: > 0 })
            {
                OpenApiTagReference openApiTagReference = references.First();

                if (openApiTagReference.Name != null)
                {
                    return openApiTagReference.Name;
                }
            }
        }

        throw new InvalidOperationException($"Failed to find tag value for endpoint '{entry.Key}'.");
    }

    private static bool IsSecondaryEndpoint(KeyValuePair<string, IOpenApiPathItem> entry)
    {
        return entry.Key.Contains("/relationships");
    }

    private static string GetRelationshipName(KeyValuePair<string, IOpenApiPathItem> entry)
    {
        Match match = RelationshipNameInUrlPattern.Match(entry.Key);

        return match.Success ? match.Groups["RelationshipName"].Value : string.Empty;
    }
}
