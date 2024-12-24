using DapperExample.TranslationToSql.DataModel;
using DapperExample.TranslationToSql.Generators;
using DapperExample.TranslationToSql.TreeNodes;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources.Annotations;

namespace DapperExample.TranslationToSql.Builders;

internal abstract class StatementBuilder
{
    private readonly IDataModelService _dataModelService;

    protected ParameterGenerator ParameterGenerator { get; } = new();

    protected StatementBuilder(IDataModelService dataModelService)
    {
        ArgumentNullException.ThrowIfNull(dataModelService);

        _dataModelService = dataModelService;
    }

    protected void ResetState()
    {
        ParameterGenerator.Reset();
    }

    protected TableNode GetTable(ResourceType resourceType, string? alias)
    {
        IReadOnlyDictionary<string, ResourceFieldAttribute?> columnMappings = _dataModelService.GetColumnMappings(resourceType);
        return new TableNode(resourceType, columnMappings, alias);
    }
}
