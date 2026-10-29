using System.Text;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries;

/// <summary>
/// A nested data structure that contains <see cref="QueryExpression" /> constraints per resource type.
/// </summary>
[PublicAPI]
public sealed class QueryLayer
{
    internal bool IsEmpty => Filter == null && Sort == null && Pagination?.PageSize == null && (Selection == null || Selection.IsEmpty);

    public ResourceType ResourceType { get; }

    /// <summary>
    /// Gets or sets the relationships to include. This only takes affect at the topmost layer.
    /// <para>
    /// The distinction between <c>null</c> and <see cref="IncludeExpression.Empty" /> is significant:
    /// <list type="bullet">
    /// <item>
    /// <term>
    /// <c>null</c>
    /// </term>
    /// <description>
    /// Omits all includes, bypassing eager loading completely (no <see cref="EagerLoadAttribute" /> annotations are loaded). Used for internal count
    /// queries, ID-existence checks, and relationship endpoints where full entity graphs are not needed.
    /// </description>
    /// </item>
    /// <item>
    /// <term>
    /// <see cref="IncludeExpression.Empty" />
    /// </term>
    /// <description>
    /// Indicates not to include any related resources, except for <see cref="EagerLoadAttribute" /> annotations on the root resource type, which will still
    /// be loaded. Used for standard queries when no
    /// <c>
    /// ?include=
    /// </c>
    /// parameter is specified.
    /// </description>
    /// </item>
    /// <item>
    /// <term>
    /// Non-empty <see cref="IncludeExpression" />
    /// </term>
    /// <description>
    /// Includes the specified relationship paths, as well as any <see cref="EagerLoadAttribute" /> annotations on the root resource and along the included
    /// relationship chains.
    /// </description>
    /// </item>
    /// </list>
    /// </para>
    /// </summary>
    public IncludeExpression? Include { get; set; }

    public FilterExpression? Filter { get; set; }
    public SortExpression? Sort { get; set; }
    public PaginationExpression? Pagination { get; set; }

    /// <summary>
    /// Gets or sets the field projection (sparse fieldsets and relationship sub-layers) to apply.
    /// <para>
    /// The distinction between <c>null</c> and an empty <see cref="FieldSelection" /> (where <see cref="FieldSelection.IsEmpty" /> is <c>true</c>) is:
    /// <list type="bullet">
    /// <item>
    /// <term>
    /// <c>null</c>
    /// </term>
    /// <description>
    /// Bypasses database-level field projection entirely; no
    /// <c>
    /// .Select(...)
    /// </c>
    /// clause is applied to the query. The data store materializes the entire entity with all mapped properties. Used when no sparse fieldset is requested
    /// or when the full database row is required (e.g. <see cref="TopFieldSelection.WithAllAttributes" />).
    /// </description>
    /// </item>
    /// <item>
    /// <term>
    /// Empty <see cref="FieldSelection" />
    /// </term>
    /// <description>
    /// Applies a database projection that selects all scalar properties on the entity type (along with any <see cref="EagerLoadAttribute" /> annotations),
    /// without restricting attributes or embedding nested relationship sub-layers.
    /// </description>
    /// </item>
    /// <item>
    /// <term>
    /// Non-empty <see cref="FieldSelection" />
    /// </term>
    /// <description>
    /// Restricts database projection to only the selected attributes and/or relationships. Unselected attributes are omitted from the database query result.
    /// </description>
    /// </item>
    /// </list>
    /// </para>
    /// </summary>
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
