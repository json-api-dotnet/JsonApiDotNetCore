using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.SwaggerComponents;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
#if NET6_0
internal sealed class EndpointOrderingFilter : IDocumentFilter
#else
internal sealed partial class EndpointOrderingFilter : IDocumentFilter
#endif
{
    private const string PatternText = $@".*{JsonApiRoutingTemplate.PrimaryEndpoint}/(?>{JsonApiRoutingTemplate.RelationshipsPart}\/)?(?<RelationshipName>\w+)";

#if NET6_0
    private const RegexOptions RegexOptionsNet60 = RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture;
    private static readonly Regex RelationshipNameInUrlPattern = new(PatternText, RegexOptionsNet60);
#endif

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        ArgumentGuard.NotNull(swaggerDoc);
        ArgumentGuard.NotNull(context);

        List<KeyValuePair<string, OpenApiPathItem>> endpointsInOrder = swaggerDoc.Paths.OrderBy(GetPrimaryResourcePublicName)
            .ThenBy(GetRelationshipName).ThenBy(IsSecondaryEndpoint).ToList();

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
        return entry.Key.Contains($"/{JsonApiRoutingTemplate.RelationshipsPart}");
    }

    private static string GetRelationshipName(KeyValuePair<string, OpenApiPathItem> entry)
    {
        Match match = RelationshipNameInUrlRegex().Match(entry.Key);

        return match.Success ? match.Groups["RelationshipName"].Value : string.Empty;
    }

#if NET6_0
    private static Regex RelationshipNameInUrlRegex()
    {
        return RelationshipNameInUrlPattern;
    }
#else
    [GeneratedRegex(PatternText, RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture)]
    private static partial Regex RelationshipNameInUrlRegex();
#endif
}
