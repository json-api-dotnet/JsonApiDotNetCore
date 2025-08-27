using System.Text;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;

namespace JsonApiDotNetCore.Queries;

/// <summary>
/// A nested data structure that contains <see cref="QueryExpression" /> constraints per resource type.
/// </summary>
[PublicAPI]
public sealed class QueryLayer
{
    internal bool IsEmpty => Filter == null && Sort == null && Pagination?.PageSize == null && (Selection == null || Selection.IsEmpty);

    public ResourceType ResourceType { get; }

    public IncludeExpression? Include { get; set; }
    public FilterExpression? Filter { get; set; }
    public SortExpression? Sort { get; set; }
    public PaginationExpression? Pagination { get; set; }
    public FieldSelection? Selection { get; set; }

    public QueryLayer(ResourceType resourceType)
    {
        ArgumentNullException.ThrowIfNull(resourceType);

        ResourceType = resourceType;
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
        WriteLayer(writer, toFullString, null);

        return builder.ToString();
    }

    internal void WriteLayer(IndentingStringWriter writer, bool toFullString, string? prefix)
    {
        writer.WriteLine($"{prefix}{nameof(QueryLayer)}<{ResourceType.ClrType.Name}>");

        using (writer.Indent())
        {
            if (Include is { Elements.Count: > 0 })
            {
                writer.WriteLine($"{nameof(Include)}: {(toFullString ? Include.ToFullString() : Include.ToString())}");
            }

            if (Filter != null)
            {
                writer.WriteLine($"{nameof(Filter)}: {(toFullString ? Filter.ToFullString() : Filter.ToString())}");
            }

            if (Sort != null)
            {
                writer.WriteLine($"{nameof(Sort)}: {(toFullString ? Sort.ToFullString() : Sort.ToString())}");
            }

            if (Pagination != null)
            {
                writer.WriteLine($"{nameof(Pagination)}: {(toFullString ? Pagination.ToFullString() : Pagination.ToString())}");
            }

            if (Selection is { IsEmpty: false })
            {
                writer.WriteLine(nameof(Selection));
                Selection.WriteSelection(writer, toFullString);
            }
        }
    }
}
