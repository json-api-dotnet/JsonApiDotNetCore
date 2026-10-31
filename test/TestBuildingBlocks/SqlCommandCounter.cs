using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace TestBuildingBlocks;

/// <summary>
/// Records the text of all SQL commands that Entity Framework Core executes, so tests can assert that (no) writes occurred.
/// </summary>
[PublicAPI]
public sealed class SqlCommandCounter : DbCommandInterceptor
{
    private readonly Lock _lockObject = new();
    private readonly List<string> _commands = [];

    public IReadOnlyList<string> Commands
    {
        get
        {
            lock (_lockObject)
            {
                return _commands.ToArray();
            }
        }
    }

    public int InsertCount => CountStartingWith("INSERT");
    public int UpdateCount => CountStartingWith("UPDATE");
    public int DeleteCount => CountStartingWith("DELETE");

    public void Reset()
    {
        lock (_lockObject)
        {
            _commands.Clear();
        }
    }

    private int CountStartingWith(string keyword)
    {
        lock (_lockObject)
        {
            return _commands.Count(command => command.TrimStart().StartsWith(keyword, StringComparison.OrdinalIgnoreCase));
        }
    }

    private void Record(DbCommand command)
    {
        lock (_lockObject)
        {
            _commands.Add(command.CommandText);
        }
    }

    public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
    {
        Record(command);
        return base.ReaderExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData,
        InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
    {
        Record(command);
        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> NonQueryExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<int> result)
    {
        Record(command);
        return base.NonQueryExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Record(command);
        return base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
    }
}
