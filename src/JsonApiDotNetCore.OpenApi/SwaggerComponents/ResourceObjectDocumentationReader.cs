using System.Collections.Concurrent;
using System.Reflection;
using System.Xml.XPath;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources.Annotations;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.SwaggerComponents;

internal sealed class ResourceObjectDocumentationReader
{
    private static readonly ConcurrentDictionary<string, XPathNavigator?> NavigatorsByAssemblyPath = new();

    public string? GetDocumentationForType(ResourceType resourceType)
    {
        ArgumentGuard.NotNull(resourceType);

        XPathNavigator? navigator = GetNavigator(resourceType.ClrType.Assembly);

        if (navigator != null)
        {
            string typeMemberName = XmlCommentsNodeNameHelper.GetMemberNameForType(resourceType.ClrType);
            return GetSummary(navigator, typeMemberName);
        }

        return null;
    }

    public string? GetDocumentationForAttribute(AttrAttribute attribute)
    {
        ArgumentGuard.NotNull(attribute);

        XPathNavigator? navigator = GetNavigator(attribute.Type.ClrType.Assembly);

        if (navigator != null)
        {
            string propertyMemberName = XmlCommentsNodeNameHelper.GetMemberNameForFieldOrProperty(attribute.Property);
            return GetSummary(navigator, propertyMemberName);
        }

        return null;
    }

    public string? GetDocumentationForRelationship(RelationshipAttribute relationship)
    {
        ArgumentGuard.NotNull(relationship);

        XPathNavigator? navigator = GetNavigator(relationship.Type.ClrType.Assembly);

        if (navigator != null)
        {
            string propertyMemberName = XmlCommentsNodeNameHelper.GetMemberNameForFieldOrProperty(relationship.Property);
            return GetSummary(navigator, propertyMemberName);
        }

        return null;
    }

    private static XPathNavigator? GetNavigator(Assembly assembly)
    {
        string assemblyPath = assembly.Location;

        if (!string.IsNullOrEmpty(assemblyPath))
        {
            return NavigatorsByAssemblyPath.GetOrAdd(assemblyPath, path =>
            {
                string documentationPath = Path.ChangeExtension(path, ".xml");

                if (File.Exists(documentationPath))
                {
                    var document = new XPathDocument(documentationPath);
                    return document.CreateNavigator();
                }

                return null;
            });
        }

        return null;
    }

    private string? GetSummary(XPathNavigator navigator, string memberName)
    {
        XPathNavigator? summaryNode = navigator.SelectSingleNode($"/doc/members/member[@name='{memberName}']/summary");
        return summaryNode != null ? XmlCommentsTextHelper.Humanize(summaryNode.InnerXml) : null;
    }
}
