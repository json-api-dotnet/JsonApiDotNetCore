namespace DapperExample.TranslationToSql.TreeNodes;

/// <summary>
/// Lists the column types used in a <see cref="TableSourceNode" />.
/// </summary>
internal enum ColumnType
{
    /// <summary>
    /// A scalar (non-relationship) column, for example: FirstName.
    /// </summary>
    Scalar,

    /// <summary>
    /// A foreign key column, for example: OwnerId.
    /// </summary>
    ForeignKey
}
