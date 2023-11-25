using DapperExample.TranslationToSql;
using JetBrains.Annotations;

namespace DapperExample.Repositories;

/// <summary>
/// Captures the emitted SQL statements, which enables integration tests to assert on them.
/// </summary>
[PublicAPI]
public sealed class SqlCaptureStore
{
    private readonly List<SqlCommand> _sqlCommands = new();

    public IReadOnlyList<SqlCommand> SqlCommands => _sqlCommands;

    public void Clear()
    {
        _sqlCommands.Clear();
    }

    internal void Add(string statement, IDictionary<string, object?>? parameters)
    {
        var sqlCommand = new SqlCommand(statement, parameters ?? new Dictionary<string, object?>());
        _sqlCommands.Add(sqlCommand);
    }
}
