using System.Text;
using DapperExample.TranslationToSql.TreeNodes;
using JsonApiDotNetCore;
using JsonApiDotNetCore.Queries.Expressions;

namespace DapperExample.TranslationToSql.Builders;

/// <summary>
/// Converts <see cref="SqlTreeNode" />s into SQL text.
/// </summary>
internal sealed class SqlQueryBuilder(DatabaseProvider databaseProvider) : SqlTreeNodeVisitor<StringBuilder, object?>
{
    private static readonly char[] SpecialCharactersInLikeDefault =
    [
        '\\',
        '%',
        '_'
    ];

    private static readonly char[] SpecialCharactersInLikeSqlServer =
    [
        '\\',
        '%',
        '_',
        '[',
        ']'
    ];

    private readonly DatabaseProvider _databaseProvider = databaseProvider;
    private readonly Dictionary<string, ParameterNode> _parametersByName = [];
    private int _indentDepth;

    private char[] SpecialCharactersInLike =>
        _databaseProvider == DatabaseProvider.SqlServer ? SpecialCharactersInLikeSqlServer : SpecialCharactersInLikeDefault;

    public IDictionary<string, object?> Parameters => _parametersByName.Values.ToDictionary(parameter => parameter.Name, parameter => parameter.Value);

    public string GetCommand(SqlTreeNode node)
    {
        ArgumentGuard.NotNull(node);

        ResetState();

        var builder = new StringBuilder();
        Visit(node, builder);
        return builder.ToString();
    }

    private void ResetState()
    {
        _parametersByName.Clear();
        _indentDepth = 0;
    }

    public override object? VisitSelect(SelectNode node, StringBuilder builder)
    {
        if (builder.Length > 0)
        {
            using (Indent())
            {
                builder.Append('(');
                WriteSelect(node, builder);
            }

            AppendOnNewLine(")", builder);
        }
        else
        {
            WriteSelect(node, builder);
        }

        WriteDeclareAlias(node.Alias, builder);
        return null;
    }

    private void WriteSelect(SelectNode node, StringBuilder builder)
    {
        AppendOnNewLine("SELECT ", builder);

        IEnumerable<SelectorNode> selectors = node.Selectors.SelectMany(selector => selector.Value);
        VisitSequence(selectors, builder);

        foreach (TableAccessorNode tableAccessor in node.Selectors.Keys)
        {
            Visit(tableAccessor, builder);
        }

        if (node.Where != null)
        {
            Visit(node.Where, builder);
        }

        if (node.OrderBy != null)
        {
            Visit(node.OrderBy, builder);
        }
    }

    public override object? VisitInsert(InsertNode node, StringBuilder builder)
    {
        AppendOnNewLine("INSERT INTO ", builder);
        Visit(node.Table, builder);
        builder.Append(" (");
        VisitSequence(node.Assignments.Select(assignment => assignment.Column), builder);
        builder.Append(')');

        ColumnNode idColumn = node.Table.GetIdColumn(node.Table.Alias);

        if (_databaseProvider == DatabaseProvider.SqlServer)
        {
            AppendOnNewLine("OUTPUT INSERTED.", builder);
            Visit(idColumn, builder);
        }

        AppendOnNewLine("VALUES (", builder);
        VisitSequence(node.Assignments.Select(assignment => assignment.Value), builder);
        builder.Append(')');

        if (_databaseProvider == DatabaseProvider.PostgreSql)
        {
            AppendOnNewLine("RETURNING ", builder);
            Visit(idColumn, builder);
        }
        else if (_databaseProvider == DatabaseProvider.MySql)
        {
            builder.Append(';');
            ColumnAssignmentNode? idAssignment = node.Assignments.FirstOrDefault(assignment => assignment.Column == idColumn);

            if (idAssignment != null)
            {
                AppendOnNewLine("SELECT ", builder);
                Visit(idAssignment.Value, builder);
            }
            else
            {
                AppendOnNewLine("SELECT LAST_INSERT_ID()", builder);
            }
        }

        return null;
    }

    public override object? VisitUpdate(UpdateNode node, StringBuilder builder)
    {
        AppendOnNewLine("UPDATE ", builder);
        Visit(node.Table, builder);

        AppendOnNewLine("SET ", builder);
        VisitSequence(node.Assignments, builder);

        Visit(node.Where, builder);
        return null;
    }

    public override object? VisitDelete(DeleteNode node, StringBuilder builder)
    {
        AppendOnNewLine("DELETE FROM ", builder);
        Visit(node.Table, builder);
        Visit(node.Where, builder);
        return null;
    }

    public override object? VisitTable(TableNode node, StringBuilder builder)
    {
        string tableName = FormatIdentifier(node.Name);
        builder.Append(tableName);
        WriteDeclareAlias(node.Alias, builder);
        return null;
    }

    public override object? VisitFrom(FromNode node, StringBuilder builder)
    {
        AppendOnNewLine("FROM ", builder);
        Visit(node.Source, builder);
        return null;
    }

    public override object? VisitJoin(JoinNode node, StringBuilder builder)
    {
        string joinTypeText = node.JoinType switch
        {
            JoinType.InnerJoin => "INNER JOIN ",
            JoinType.LeftJoin => "LEFT JOIN ",
            _ => throw new NotSupportedException($"Unknown join type '{node.JoinType}'.")
        };

        AppendOnNewLine(joinTypeText, builder);
        Visit(node.Source, builder);
        builder.Append(" ON ");
        Visit(node.OuterColumn, builder);
        builder.Append(" = ");
        Visit(node.InnerColumn, builder);
        return null;
    }

    public override object? VisitColumnInTable(ColumnInTableNode node, StringBuilder builder)
    {
        WriteColumn(node, false, builder);
        return null;
    }

    public override object? VisitColumnInSelect(ColumnInSelectNode node, StringBuilder builder)
    {
        WriteColumn(node, node.IsVirtual, builder);
        return null;
    }

    private void WriteColumn(ColumnNode column, bool isVirtualColumn, StringBuilder builder)
    {
        WriteReferenceAlias(column.TableAlias, builder);

        string name = isVirtualColumn ? column.Name : FormatIdentifier(column.Name);
        builder.Append(name);
    }

    public override object? VisitColumnSelector(ColumnSelectorNode node, StringBuilder builder)
    {
        Visit(node.Column, builder);
        WriteDeclareAlias(node.Alias, builder);
        return null;
    }

    public override object? VisitOneSelector(OneSelectorNode node, StringBuilder builder)
    {
        builder.Append('1');
        WriteDeclareAlias(node.Alias, builder);
        return null;
    }

    public override object? VisitCountSelector(CountSelectorNode node, StringBuilder builder)
    {
        builder.Append("COUNT(*)");
        WriteDeclareAlias(node.Alias, builder);
        return null;
    }

    public override object? VisitWhere(WhereNode node, StringBuilder builder)
    {
        AppendOnNewLine("WHERE ", builder);
        Visit(node.Filter, builder);
        return null;
    }

    public override object? VisitNot(NotNode node, StringBuilder builder)
    {
        builder.Append("NOT (");
        Visit(node.Child, builder);
        builder.Append(')');
        return null;
    }

    public override object? VisitLogical(LogicalNode node, StringBuilder builder)
    {
        string operatorText = node.Operator switch
        {
            LogicalOperator.And => "AND",
            LogicalOperator.Or => "OR",
            _ => throw new NotSupportedException($"Unknown logical operator '{node.Operator}'.")
        };

        builder.Append('(');
        Visit(node.Terms[0], builder);
        builder.Append(')');

        foreach (FilterNode nextTerm in node.Terms.Skip(1))
        {
            builder.Append($" {operatorText} (");
            Visit(nextTerm, builder);
            builder.Append(')');
        }

        return null;
    }

    public override object? VisitComparison(ComparisonNode node, StringBuilder builder)
    {
        string operatorText = node.Operator switch
        {
            ComparisonOperator.Equals => node.Left is NullConstantNode || node.Right is NullConstantNode ? "IS" : "=",
            ComparisonOperator.GreaterThan => ">",
            ComparisonOperator.GreaterOrEqual => ">=",
            ComparisonOperator.LessThan => "<",
            ComparisonOperator.LessOrEqual => "<=",
            _ => throw new NotSupportedException($"Unknown comparison operator '{node.Operator}'.")
        };

        Visit(node.Left, builder);
        builder.Append($" {operatorText} ");
        Visit(node.Right, builder);
        return null;
    }

    public override object? VisitLike(LikeNode node, StringBuilder builder)
    {
        Visit(node.Column, builder);
        builder.Append(" LIKE '");

        if (node.MatchKind is TextMatchKind.Contains or TextMatchKind.EndsWith)
        {
            builder.Append('%');
        }

        string safeValue = node.Text.Replace("'", "''");
        bool requireEscapeClause = node.Text.IndexOfAny(SpecialCharactersInLike) != -1;

        if (requireEscapeClause)
        {
            foreach (char specialCharacter in SpecialCharactersInLike)
            {
                safeValue = safeValue.Replace(specialCharacter.ToString(), @"\" + specialCharacter);
            }
        }

        if (requireEscapeClause && _databaseProvider == DatabaseProvider.MySql)
        {
            safeValue = safeValue.Replace(@"\\", @"\\\\");
        }

        builder.Append(safeValue);

        if (node.MatchKind is TextMatchKind.Contains or TextMatchKind.StartsWith)
        {
            builder.Append('%');
        }

        builder.Append('\'');

        if (requireEscapeClause)
        {
            builder.Append(_databaseProvider == DatabaseProvider.MySql ? @" ESCAPE '\\'" : @" ESCAPE '\'");
        }

        return null;
    }

    public override object? VisitIn(InNode node, StringBuilder builder)
    {
        Visit(node.Column, builder);
        builder.Append(" IN (");
        VisitSequence(node.Values, builder);
        builder.Append(')');
        return null;
    }

    public override object? VisitExists(ExistsNode node, StringBuilder builder)
    {
        builder.Append("EXISTS ");
        Visit(node.SubSelect, builder);
        return null;
    }

    public override object? VisitCount(CountNode node, StringBuilder builder)
    {
        Visit(node.SubSelect, builder);
        return null;
    }

    public override object? VisitOrderBy(OrderByNode node, StringBuilder builder)
    {
        AppendOnNewLine("ORDER BY ", builder);
        VisitSequence(node.Terms, builder);
        return null;
    }

    public override object? VisitOrderByColumn(OrderByColumnNode node, StringBuilder builder)
    {
        Visit(node.Column, builder);

        if (!node.IsAscending)
        {
            builder.Append(" DESC");
        }

        return null;
    }

    public override object? VisitOrderByCount(OrderByCountNode node, StringBuilder builder)
    {
        Visit(node.Count, builder);

        if (!node.IsAscending)
        {
            builder.Append(" DESC");
        }

        return null;
    }

    public override object? VisitColumnAssignment(ColumnAssignmentNode node, StringBuilder builder)
    {
        Visit(node.Column, builder);
        builder.Append(" = ");
        Visit(node.Value, builder);
        return null;
    }

    public override object? VisitParameter(ParameterNode node, StringBuilder builder)
    {
        _parametersByName[node.Name] = node;

        builder.Append(node.Name);
        return null;
    }

    public override object? VisitNullConstant(NullConstantNode node, StringBuilder builder)
    {
        builder.Append("NULL");
        return null;
    }

    private static void WriteDeclareAlias(string? alias, StringBuilder builder)
    {
        if (alias != null)
        {
            builder.Append($" AS {alias}");
        }
    }

    private static void WriteReferenceAlias(string? alias, StringBuilder builder)
    {
        if (alias != null)
        {
            builder.Append($"{alias}.");
        }
    }

    private void VisitSequence<T>(IEnumerable<T> elements, StringBuilder builder)
        where T : SqlTreeNode
    {
        bool isFirstElement = true;

        foreach (T element in elements)
        {
            if (isFirstElement)
            {
                isFirstElement = false;
            }
            else
            {
                builder.Append(", ");
            }

            Visit(element, builder);
        }
    }

    private void AppendOnNewLine(string? value, StringBuilder builder)
    {
        if (!string.IsNullOrEmpty(value))
        {
            if (builder.Length > 0)
            {
                builder.AppendLine();
            }

            builder.Append(new string(' ', _indentDepth * 4));
            builder.Append(value);
        }
    }

    private string FormatIdentifier(string value)
    {
        return FormatIdentifier(value, _databaseProvider);
    }

    internal static string FormatIdentifier(string value, DatabaseProvider databaseProvider)
    {
        return databaseProvider switch
        {
            // https://www.postgresql.org/docs/current/sql-syntax-lexical.html
            DatabaseProvider.PostgreSql => $"\"{value.Replace("\"", "\"\"")}\"",
            // https://dev.mysql.com/doc/refman/8.0/en/identifiers.html
            DatabaseProvider.MySql => $"`{value.Replace("`", "``")}`",
            // https://learn.microsoft.com/en-us/sql/t-sql/functions/quotename-transact-sql?view=sql-server-ver16
            DatabaseProvider.SqlServer => $"[{value.Replace("]", "]]")}]",
            _ => throw new NotSupportedException($"Unknown database provider '{databaseProvider}'.")
        };
    }

    private IDisposable Indent()
    {
        _indentDepth++;
        return new RevertIndentOnDispose(this);
    }

    private sealed class RevertIndentOnDispose(SqlQueryBuilder owner) : IDisposable
    {
        private readonly SqlQueryBuilder _owner = owner;

        public void Dispose()
        {
            _owner._indentDepth--;
        }
    }
}
