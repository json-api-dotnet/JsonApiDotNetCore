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

    public ISet<ResourceType> GetResourceTypes()
    {
        return Keys.ToHashSet();
    }

#pragma warning disable AV1130 // Return type in method signature should be a collection interface instead of a concrete type
    public FieldSelectors GetOrCreateSelectors(ResourceType resourceType)
#pragma warning restore AV1130 // Return type in method signature should be a collection interface instead of a concrete type
    {
        ArgumentGuard.NotNull(resourceType, nameof(resourceType));

        if (!ContainsKey(resourceType))
        {
            this[resourceType] = new FieldSelectors();
        }

        return this[resourceType];
    }

    public override string ToString()
    {
        var builder = new StringBuilder();

        var writer = new IndentingStringWriter(builder);
        WriteSelection(writer);

        return builder.ToString();
    }

    internal void WriteSelection(IndentingStringWriter writer)
    {
        using (writer.Indent())
        {
            foreach (ResourceType type in GetResourceTypes())
            {
                writer.WriteLine($"{nameof(FieldSelectors)}<{type.ClrType.Name}>");
                WriterSelectors(writer, type);
            }
        }
    }

    private void WriterSelectors(IndentingStringWriter writer, ResourceType type)
    {
        using (writer.Indent())
        {
            foreach ((ResourceFieldAttribute field, QueryLayer? nextLayer) in GetOrCreateSelectors(type))
            {
                if (nextLayer == null)
                {
                    writer.WriteLine(field.ToString());
                }
                else
                {
                    nextLayer.WriteLayer(writer, $"{field.PublicName}: ");
                }
            }
        }
    }
}
