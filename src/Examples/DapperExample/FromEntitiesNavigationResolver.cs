using DapperExample.Data;
using DapperExample.TranslationToSql.DataModel;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DapperExample;

/// <summary>
/// Resolves inverse navigations and initializes <see cref="IDataModelService" /> from an Entity Framework Core <see cref="DbContext" />.
/// </summary>
internal sealed class FromEntitiesNavigationResolver : IInverseNavigationResolver
{
    private readonly InverseNavigationResolver _defaultResolver;
    private readonly FromEntitiesDataModelService _dataModelService;
    private readonly DbContext _appDbContext;

    public FromEntitiesNavigationResolver(IResourceGraph resourceGraph, FromEntitiesDataModelService dataModelService, AppDbContext appDbContext)
    {
        ArgumentNullException.ThrowIfNull(resourceGraph);
        ArgumentNullException.ThrowIfNull(dataModelService);
        ArgumentNullException.ThrowIfNull(appDbContext);

        _defaultResolver = new InverseNavigationResolver(resourceGraph, new[]
        {
            new DbContextResolver<AppDbContext>(appDbContext)
        });

        _dataModelService = dataModelService;
        _appDbContext = appDbContext;
    }

    public void Resolve()
    {
        // To produce SQL, some knowledge of the underlying database model is required.
        // Because the database in this example project is created using Entity Framework Core, we derive that information from its model.
        // Some alternative approaches to consider:
        // - Query the database to obtain model information at startup.
        // - Create a custom attribute that is put on [HasOne/HasMany] resource properties and scan for them at startup.
        // - Hard-code the required information in the application.

        _defaultResolver.Resolve();
        _dataModelService.Initialize(_appDbContext);
    }
}
