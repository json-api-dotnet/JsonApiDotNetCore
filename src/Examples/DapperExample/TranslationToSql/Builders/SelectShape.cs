namespace DapperExample.TranslationToSql.Builders;

/// <summary>
/// Indicates what to select in a SELECT statement.
/// </summary>
internal enum SelectShape
{
    /// <summary>
    /// Select a set of columns.
    /// </summary>
    Columns,

    /// <summary>
    /// Select the number of rows: COUNT(*).
    /// </summary>
    Count,

    /// <summary>
    /// Select only the first, unnamed column: SELECT 1.
    /// </summary>
    One
}
