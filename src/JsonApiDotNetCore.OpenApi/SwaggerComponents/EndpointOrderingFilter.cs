using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.SwaggerComponents;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
internal sealed class EndpointOrderingFilter : IDocumentFilter
{
    private static readonly Regex RelationshipNameInUrlPattern =
        new($".*{JsonApiRoutingTemplate.PrimaryEndpoint}/(?>{JsonApiRoutingTemplate.RelationshipsPart}\\/)?(\\w+)", RegexOptions.Compiled);

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        ArgumentGuard.NotNull(swaggerDoc);
        ArgumentGuard.NotNull(context);

        List<KeyValuePair<string, OpenApiPathItem>> orderedEndpoints = swaggerDoc.Paths.OrderBy(GetPrimaryResourcePublicName)
            .ThenBy(GetRelationshipName).ThenBy(IsSecondaryEndpoint).ToList();

        swaggerDoc.Paths.Clear();

        foreach ((string url, OpenApiPathItem path) in orderedEndpoints)
        {
            swaggerDoc.Paths.Add(url, path);
        }
    }

    private static string GetPrimaryResourcePublicName(KeyValuePair<string, OpenApiPathItem> entry)
    {
        return entry.Value.Operations.First().Value.Tags.First().Name;
    }

    private static bool IsSecondaryEndpoint(KeyValuePair<string, OpenApiPathItem> entry)
    {
        return entry.Key.Contains("/" + JsonApiRoutingTemplate.RelationshipsPart);
    }

    private static string GetRelationshipName(KeyValuePair<string, OpenApiPathItem> entry)
    {
        Match match = RelationshipNameInUrlPattern.Match(entry.Key);

        return match.Success ? match.Groups[1].Value : string.Empty;
    }
}
