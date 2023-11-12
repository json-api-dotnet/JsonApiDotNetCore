namespace DapperExample.TranslationToSql.Transformations;

internal enum ColumnVisitMode
{
    /// <summary>
    /// Definition of a column in a SQL query.
    /// </summary>
    Declaration,

    /// <summary>
    /// Usage of a column in a SQL query.
    /// </summary>
    Reference
}
