using System.Text;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries;

/// <summary>
/// Provides access to sparse fieldsets, per resource type. There's usually just a single resource type, but there can be multiple in case an endpoint
/// for an abstract resource type returns derived types.
/// </summary>
[PublicAPI]
public sealed class FieldSelection : Dictionary<ResourceType, FieldSelectors>
{
    public bool IsEmpty => Values.All(selectors => selectors.IsEmpty);

    public IReadOnlySet<ResourceType> GetResourceTypes()
    {
        return Keys.ToHashSet().AsReadOnly();
    }

    public FieldSelectors GetOrCreateSelectors(ResourceType resourceType)
    {
        ArgumentNullException.ThrowIfNull(resourceType);

        if (!ContainsKey(resourceType))
        {
            this[resourceType] = new FieldSelectors();
        }

        return this[resourceType];
    }

    public override string ToString()
    {
        return InnerToString(false);
    }

    public string ToFullString()
    {
        return InnerToString(true);
    }

    private string InnerToString(bool toFullString)
    {
        var builder = new StringBuilder();

        var writer = new IndentingStringWriter(builder);
        WriteSelection(writer, toFullString);

        return builder.ToString();
    }

    internal void WriteSelection(IndentingStringWriter writer, bool toFullString)
    {
        using (writer.Indent())
        {
            foreach (ResourceType type in GetResourceTypes())
            {
                writer.WriteLine($"{nameof(FieldSelectors)}<{type.ClrType.Name}>");
                WriteSelectors(writer, toFullString, type);
            }
        }
    }

    private void WriteSelectors(IndentingStringWriter writer, bool toFullString, ResourceType type)
    {
        FieldSelectors selectors = GetOrCreateSelectors(type);

        using (writer.Indent())
        {
            foreach ((ResourceFieldAttribute field, QueryLayer? nextLayer) in selectors)
            {
                if (nextLayer == null)
                {
                    writer.WriteLine(toFullString ? field.ToFullString() : field.ToString());

                    if (field is AttrAttribute attr && selectors.GetChildSelectors(attr) is { } childSelectors)
                    {
                        WriteChildSelectors(writer, toFullString, childSelectors);
                    }
                }
                else
                {
                    string prefix = $"{(toFullString ? field.ToFullString() : field.ToString())}: ";
                    nextLayer.WriteLayer(writer, toFullString, prefix);
                }
            }
        }
    }

    private static void WriteChildSelectors(IndentingStringWriter writer, bool toFullString, FieldSelectors childSelectors)
    {
        using (writer.Indent())
        {
            foreach (ResourceFieldAttribute field in childSelectors.Keys)
            {
                writer.WriteLine(toFullString ? field.ToFullString() : field.ToString());

                if (field is AttrAttribute attr && childSelectors.GetChildSelectors(attr) is { } nextChildren)
                {
                    WriteChildSelectors(writer, toFullString, nextChildren);
                }
            }
        }
    }
}
