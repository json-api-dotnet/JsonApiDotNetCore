using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.SwaggerComponents;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
internal sealed partial class EndpointOrderingFilter : IDocumentFilter
{
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
            ISet<OpenApiTagReference>? tagReferences = entry.Value.Operations.First().Value.Tags;

            if (tagReferences is { Count: > 0 })
            {
                OpenApiTagReference openApiTagReference = tagReferences.First();

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
        Match match = RelationshipNameInUrlRegex().Match(entry.Key);

        return match.Success ? match.Groups["RelationshipName"].Value : string.Empty;
    }

    [GeneratedRegex(@".*{id}/(?>relationships\/)?(?<RelationshipName>\w+)", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture)]
    private static partial Regex RelationshipNameInUrlRegex();
}
