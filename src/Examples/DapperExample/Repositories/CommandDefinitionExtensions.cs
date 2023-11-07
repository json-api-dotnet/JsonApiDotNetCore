using System.Data.Common;
using Dapper;
using DapperExample.AtomicOperations;

namespace DapperExample.Repositories;

internal static class CommandDefinitionExtensions
{
    // SQL Server and MySQL require any active DbTransaction to be explicitly associated to the DbConnection.

    public static CommandDefinition Associate(this CommandDefinition command, DbTransaction transaction)
    {
        return new CommandDefinition(command.CommandText, command.Parameters, transaction, cancellationToken: command.CancellationToken);
    }

    public static CommandDefinition Associate(this CommandDefinition command, AmbientTransaction? transaction)
    {
        return transaction != null
            ? new CommandDefinition(command.CommandText, command.Parameters, transaction.Current, cancellationToken: command.CancellationToken)
            : command;
    }
}
