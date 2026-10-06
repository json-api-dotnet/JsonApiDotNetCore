using System.Text;
using DapperExample.TranslationToSql.Builders;
using Humanizer;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources.Annotations;

namespace DapperExample.TranslationToSql.DataModel;

/// <summary>
/// Defines foreign key information for a <see cref="RelationshipAttribute" />, which is required to produce SQL queries.
/// </summary>
[PublicAPI]
public sealed class RelationshipForeignKey
{
    private readonly DatabaseProvider _databaseProvider;

    /// <summary>
    /// The JSON:API relationship mapped to this foreign key.
    /// </summary>
    public RelationshipAttribute Relationship { get; }

    /// <summary>
    /// Indicates whether the foreign key column is defined on the left side of the JSON:API relationship.
    /// </summary>
    public bool IsAtLeftSide { get; }

    /// <summary>
    /// The foreign key column name.
    /// </summary>
    public string ColumnName { get; }

    /// <summary>
    /// Indicates whether the foreign key column is nullable.
    /// </summary>
    public bool IsNullable { get; }

    /// <summary>
    /// Indicates whether the foreign key represents a one-to-one relationship.
    /// </summary>
    public bool IsOneToOne { get; }

    /// <summary>
    /// Indicates whether the foreign key column must be set to null client-side before deleting the principal resource.
    /// </summary>
    public bool UseClientSetNullOnDelete { get; }

    public RelationshipForeignKey(DatabaseProvider databaseProvider, RelationshipAttribute relationship, bool isAtLeftSide, string columnName, bool isNullable,
        bool isOneToOne, bool useClientSetNullOnDelete)
    {
        ArgumentNullException.ThrowIfNull(relationship);
        ArgumentException.ThrowIfNullOrEmpty(columnName);

        _databaseProvider = databaseProvider;
        Relationship = relationship;
        IsAtLeftSide = isAtLeftSide;
        ColumnName = columnName;
        IsNullable = isNullable;
        IsOneToOne = isOneToOne;
        UseClientSetNullOnDelete = useClientSetNullOnDelete;
    }

    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.Append($"{Relationship.LeftType.ClrType.Name}.{Relationship.Property.Name} ({GetMultiplicity()}) -> ");

        ResourceType tableType = IsAtLeftSide ? Relationship.LeftType : Relationship.RightType;

        builder.Append(SqlQueryBuilder.FormatIdentifier(tableType.ClrType.Name.Pluralize(), _databaseProvider));
        builder.Append('.');
        builder.Append(SqlQueryBuilder.FormatIdentifier(ColumnName, _databaseProvider));

        if (IsNullable)
        {
            builder.Append('?');
        }

        if (UseClientSetNullOnDelete)
        {
            builder.Append(" (OnDelete=ClientSetNull)");
        }

        return builder.ToString();
    }

    private string GetMultiplicity()
    {
        string multiplicityAtPrincipalSide = IsNullable ? "0..1" : "1";
        string multiplicityAtDependentSide = IsOneToOne ? "1" : "*";

        return IsAtLeftSide ? $"{multiplicityAtPrincipalSide}:{multiplicityAtDependentSide}" : $"{multiplicityAtDependentSide}:{multiplicityAtPrincipalSide}";
    }
}
