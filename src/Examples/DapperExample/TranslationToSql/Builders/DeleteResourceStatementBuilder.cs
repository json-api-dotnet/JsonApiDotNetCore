using System.Collections.ObjectModel;
using DapperExample.TranslationToSql.DataModel;
using DapperExample.TranslationToSql.TreeNodes;
using JsonApiDotNetCore;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;

namespace DapperExample.TranslationToSql.Builders;

internal sealed class DeleteResourceStatementBuilder(IDataModelService dataModelService) : StatementBuilder(dataModelService)
{
    public DeleteNode Build(ResourceType resourceType, params object[] idValues)
    {
        ArgumentGuard.NotNull(resourceType);
        ArgumentGuard.NotNullNorEmpty(idValues);

        ResetState();

        TableNode table = GetTable(resourceType, null);

        ColumnNode idColumn = table.GetIdColumn(table.Alias);
        WhereNode where = GetWhere(idColumn, idValues);

        return new DeleteNode(table, where);
    }

    private WhereNode GetWhere(ColumnNode idColumn, IEnumerable<object> idValues)
    {
        ReadOnlyCollection<ParameterNode> parameters = idValues.Select(idValue => ParameterGenerator.Create(idValue)).ToArray().AsReadOnly();
        FilterNode filter = parameters.Count == 1 ? new ComparisonNode(ComparisonOperator.Equals, idColumn, parameters[0]) : new InNode(idColumn, parameters);
        return new WhereNode(filter);
    }
}
