using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.SwaggerComponents;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
internal sealed partial class EndpointOrderingFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        ArgumentNullException.ThrowIfNull(swaggerDoc);
        ArgumentNullException.ThrowIfNull(context);

        KeyValuePair<string, OpenApiPathItem>[] endpointsInOrder = swaggerDoc.Paths.OrderBy(GetPrimaryResourcePublicName)
            .ThenBy(GetRelationshipName).ThenBy(IsSecondaryEndpoint).ToArray();

        swaggerDoc.Paths.Clear();

        foreach ((string url, OpenApiPathItem path) in endpointsInOrder)
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
        return entry.Key.Contains("/relationships");
    }

    private static string GetRelationshipName(KeyValuePair<string, OpenApiPathItem> entry)
    {
        Match match = RelationshipNameInUrlRegex().Match(entry.Key);

        return match.Success ? match.Groups["RelationshipName"].Value : string.Empty;
    }

    [GeneratedRegex(@".*{id}/(?>relationships\/)?(?<RelationshipName>\w+)", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture)]
    private static partial Regex RelationshipNameInUrlRegex();
}
