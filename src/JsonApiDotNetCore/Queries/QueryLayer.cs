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
    public ResourceType ResourceType { get; }

    public IncludeExpression? Include { get; set; }
    public FilterExpression? Filter { get; set; }
    public SortExpression? Sort { get; set; }
    public PaginationExpression? Pagination { get; set; }
    public FieldSelection? Selection { get; set; }

    public QueryLayer(ResourceType resourceType)
    {
        ArgumentGuard.NotNull(resourceType, nameof(resourceType));

        ResourceType = resourceType;
    }

    public override string ToString()
    {
        var builder = new StringBuilder();

        var writer = new IndentingStringWriter(builder);
        WriteLayer(writer, null);

        return builder.ToString();
    }

    internal void WriteLayer(IndentingStringWriter writer, string? prefix)
    {
        writer.WriteLine($"{prefix}{nameof(QueryLayer)}<{ResourceType.ClrType.Name}>");

        using (writer.Indent())
        {
            if (Include != null)
            {
                writer.WriteLine($"{nameof(Include)}: {Include}");
            }

            if (Filter != null)
            {
                writer.WriteLine($"{nameof(Filter)}: {Filter}");
            }

            if (Sort != null)
            {
                writer.WriteLine($"{nameof(Sort)}: {Sort}");
            }

            if (Pagination != null)
            {
                writer.WriteLine($"{nameof(Pagination)}: {Pagination}");
            }

            if (Selection is { IsEmpty: false })
            {
                writer.WriteLine(nameof(Selection));
                Selection.WriteSelection(writer);
            }
        }
    }
}
