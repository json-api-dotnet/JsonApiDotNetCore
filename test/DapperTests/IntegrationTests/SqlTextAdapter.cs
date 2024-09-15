using System.Text.RegularExpressions;
using DapperExample;

namespace DapperTests.IntegrationTests;

internal sealed class SqlTextAdapter(DatabaseProvider databaseProvider)
{
#if NET6_0
    private const RegexOptions Options = RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture;
#else
    private const RegexOptions Options = RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.NonBacktracking;
#endif

    private static readonly Dictionary<Regex, string> SqlServerReplacements = new()
    {
        [new Regex("\"([^\"]+)\"", Options)] = "[$+]",
        [new Regex($@"(VALUES \([^)]*\)){Environment.NewLine}RETURNING \[Id\]", Options)] = $"OUTPUT INSERTED.[Id]{Environment.NewLine}$1"
    };

    private readonly DatabaseProvider _databaseProvider = databaseProvider;

    public string Adapt(string text, bool hasClientGeneratedId)
    {
        string replaced = text;

        if (_databaseProvider == DatabaseProvider.MySql)
        {
            replaced = replaced.Replace(@"""", "`");

            string selectInsertId = hasClientGeneratedId ? $";{Environment.NewLine}SELECT @p1" : $";{Environment.NewLine}SELECT LAST_INSERT_ID()";
            replaced = replaced.Replace($"{Environment.NewLine}RETURNING `Id`", selectInsertId);

            replaced = replaced.Replace(@"\\", @"\\\\").Replace(@" ESCAPE '\'", @" ESCAPE '\\'");
        }
        else if (_databaseProvider == DatabaseProvider.SqlServer)
        {
            foreach ((Regex regex, string replacementPattern) in SqlServerReplacements)
            {
                replaced = regex.Replace(replaced, replacementPattern);
            }
        }

        return replaced;
    }
}
