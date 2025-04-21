using JetBrains.Annotations;

namespace DapperExample.TranslationToSql;

/// <summary>
/// Represents a parameterized SQL query.
/// </summary>
[PublicAPI]
public sealed class SqlCommand
{
    public string Statement { get; }
    public IDictionary<string, object?> Parameters { get; }

    internal SqlCommand(string statement, IDictionary<string, object?> parameters)
    {
        ArgumentNullException.ThrowIfNull(statement);
        ArgumentNullException.ThrowIfNull(parameters);

        Statement = statement;
        Parameters = parameters;
    }
}
