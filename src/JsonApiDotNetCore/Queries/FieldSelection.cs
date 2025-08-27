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

#pragma warning disable AV1130 // Return type in method signature should be an interface to an unchangeable collection
    public FieldSelectors GetOrCreateSelectors(ResourceType resourceType)
#pragma warning restore AV1130 // Return type in method signature should be an interface to an unchangeable collection
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
                WriterSelectors(writer, toFullString, type);
            }
        }
    }

    private void WriterSelectors(IndentingStringWriter writer, bool toFullString, ResourceType type)
    {
        using (writer.Indent())
        {
            foreach ((ResourceFieldAttribute field, QueryLayer? nextLayer) in GetOrCreateSelectors(type))
            {
                if (nextLayer == null)
                {
                    writer.WriteLine(toFullString ? field.ToFullString() : field.ToString());
                }
                else
                {
                    string prefix = $"{(toFullString ? field.ToFullString() : field.ToString())}: ";
                    nextLayer.WriteLayer(writer, toFullString, prefix);
                }
            }
        }
    }
}
