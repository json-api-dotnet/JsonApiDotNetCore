namespace DapperExample.TranslationToSql.Generators;

/// <summary>
/// Generates a SQL table alias with a unique name.
/// </summary>
internal sealed class TableAliasGenerator : UniqueNameGenerator
{
    public TableAliasGenerator()
        : base("t")
    {
    }
}
