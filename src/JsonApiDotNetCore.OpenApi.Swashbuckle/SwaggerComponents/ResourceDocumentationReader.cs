using System.Collections.Concurrent;
using System.Reflection;
using System.Xml;
using System.Xml.XPath;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources.Annotations;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.SwaggerComponents;

internal sealed class ResourceDocumentationReader
{
    private static readonly ConcurrentDictionary<string, XPathNavigator?> NavigatorsByAssemblyPath = new();

    public string? GetDocumentationForType(ResourceType resourceType)
    {
        ArgumentNullException.ThrowIfNull(resourceType);

        var navigator = GetNavigator(resourceType.ClrType.Assembly);

        if (navigator != null)
        {
            var typeMemberName = XmlCommentsNodeNameHelper.GetMemberNameForType(resourceType.ClrType);
            return GetSummary(navigator, typeMemberName);
        }

        return null;
    }

    public string? GetDocumentationForAttribute(AttrAttribute attribute)
    {
        ArgumentNullException.ThrowIfNull(attribute);

        var navigator = GetNavigator(attribute.Type.ClrType.Assembly);

        if (navigator != null)
        {
            var propertyMemberName = XmlCommentsNodeNameHelper.GetMemberNameForFieldOrProperty(attribute.Property);
            return GetSummary(navigator, propertyMemberName);
        }

        return null;
    }

    public string? GetDocumentationForRelationship(RelationshipAttribute relationship)
    {
        ArgumentNullException.ThrowIfNull(relationship);

        var navigator = GetNavigator(relationship.Type.ClrType.Assembly);

        if (navigator != null)
        {
            var propertyMemberName = XmlCommentsNodeNameHelper.GetMemberNameForFieldOrProperty(relationship.Property);
            return GetSummary(navigator, propertyMemberName);
        }

        return null;
    }

    private static XPathNavigator? GetNavigator(Assembly assembly)
    {
        var assemblyPath = assembly.Location;

        if (!string.IsNullOrEmpty(assemblyPath))
        {
            return NavigatorsByAssemblyPath.GetOrAdd(assemblyPath, path =>
            {
                var documentationPath = Path.ChangeExtension(path, ".xml");

                if (File.Exists(documentationPath))
                {
                    using var reader = XmlReader.Create(documentationPath);
                    var document = new XPathDocument(reader);
                    return document.CreateNavigator();
                }

                return null;
            });
        }

        return null;
    }

    private string? GetSummary(XPathNavigator navigator, string memberName)
    {
        var summaryNode = navigator.SelectSingleNode($"/doc/members/member[@name='{memberName}']/summary");
        return summaryNode != null ? XmlCommentsTextHelper.Humanize(summaryNode.InnerXml) : null;
    }
}
