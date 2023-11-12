using JetBrains.Annotations;
using JsonApiDotNetCore;

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
        ArgumentGuard.NotNull(statement);
        ArgumentGuard.NotNull(parameters);

        Statement = statement;
        Parameters = parameters;
    }
}
