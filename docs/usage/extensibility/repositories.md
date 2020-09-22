# Resource Repositories

If you want to use a data access technology other than Entity Framework Core, you can create an implementation of IResourceRepository<TResource, TId>.
If you only need minor changes you can override the methods defined in EntityFrameworkCoreRepository<TResource, TId>.

The repository should then be added to the service collection in Startup.cs.

```c#
public void ConfigureServices(IServiceCollection services)
{
    services.AddScoped<IResourceRepository<Article>, ArticleRepository>();
    // ...
}
```

A sample implementation that performs authorization might look like this.

All of the methods in EntityFrameworkCoreRepository will use the GetAll() method to get the DbSet<TResource>, so this is a good method to apply filters such as user or tenant authorization.

```c#
public class ArticleRepository : EntityFrameworkCoreRepository<Article>
{
    private readonly IAuthenticationService _authenticationService;

    public ArticleRepository(
        IAuthenticationService authenticationService,
        ITargetedFields targetedFields,
        IDbContextResolver contextResolver,
        IResourceGraph resourceGraph,
        IGenericServiceFactory genericServiceFactory,
        IResourceFactory resourceFactory,
        IEnumerable<IQueryConstraintProvider> constraintProviders,
        ILoggerFactory loggerFactory)
        : base(targetedFields, contextResolver, resourceGraph, genericServiceFactory, resourceFactory, constraintProviders, loggerFactory) 
    {
        _authenticationService = authenticationService;
    }

    public override IQueryable<Article> GetAll()
    {
        return base.Get().Where(article => article.UserId == _authenticationService.UserId);
    }
}
```

## Multiple DbContexts

If you need to use multiple Entity Framework Core DbContexts, first create an implementation of `IDbContextResolver` for each context.

```c#
public sealed class DbContextAResolver : IDbContextResolver
{
    private readonly DbContextA _dbContextA;

    public DbContextAResolver(DbContextA dbContextA)
    {
        _dbContextA = dbContextA;
    }

    public DbContext GetContext()
    {
        return _dbContextA;
    }
}
```

Next, create a repository for each context and inject its resolver per resource type. This example shows a single `DbContextARepository` for all entities that are members of `DbContextA`.

```c#
public class DbContextARepository<TResource> : EntityFrameworkCoreRepository<TResource>
    where TResource : class, IIdentifiable<int>
{
    public DbContextARepository(ITargetedFields targetedFields, DbContextAResolver contextResolver,
        //                                                      ^^^^^^^^^^^^^^^^^^
        IResourceGraph resourceGraph, IGenericServiceFactory genericServiceFactory,
        IResourceFactory resourceFactory, IEnumerable<IQueryConstraintProvider> constraintProviders,
        ILoggerFactory loggerFactory)
        : base(targetedFields, contextResolver, resourceGraph, genericServiceFactory, resourceFactory,
            constraintProviders, loggerFactory)
    {
    }
}

```

Then register the added types and use the non-generic overload of `AddJsonApi` to add their resources to the graph.

```c#
// Startup.ConfigureServices
services.AddScoped<DbContextAResolver>();
services.AddScoped<DbContextBResolver>();

services.AddScoped<IResourceRepository<ResourceA>, DbContextARepository<ResourceA>>();
services.AddScoped<IResourceRepository<ResourceB>, DbContextBRepository<ResourceB>>();

services.AddJsonApi(dbContextTypes: new[] {typeof(DbContextA), typeof(DbContextB)});
```
