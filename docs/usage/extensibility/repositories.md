# Resource Repositories

If you want to use a data access technology other than Entity Framework Core, you can create an implementation of `IResourceRepository<TResource, TId>`.
If you only need minor changes you can override the methods defined in `EntityFrameworkCoreRepository<TResource, TId>`.

The repository should then be registered in Startup.cs.

```c#
public void ConfigureServices(IServiceCollection services)
{
    services.AddScoped<IResourceRepository<Article>, ArticleRepository>();
}
```

In v4.0 we introduced an extension method that you can use to register a resource repository on all of its JsonApiDotNetCore interfaces.
This is helpful when you implement a subset of the resource interfaces and want to register them all in one go.

Note: If you're using service discovery, this happens automatically.

```c#
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddResourceRepository<ArticleRepository>();
    }
}
```

A sample implementation that performs authorization might look like this.

All of the methods in EntityFrameworkCoreRepository will use the `GetAll()` method to get the `DbSet<TResource>`, so this is a good method to apply filters such as user or tenant authorization.

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
        : base(targetedFields, contextResolver, resourceGraph, genericServiceFactory,
            resourceFactory, constraintProviders, loggerFactory) 
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

If you need to use multiple Entity Framework Core DbContexts, first create a repository for each context and inject its typed resolver.
This example shows a single `DbContextARepository` for all entities that are members of `DbContextA`.

```c#
public class DbContextARepository<TResource> : EntityFrameworkCoreRepository<TResource>
    where TResource : class, IIdentifiable<int>
{
    public DbContextARepository(ITargetedFields targetedFields, DbContextResolver<DbContextA> contextResolver,
        //                                                      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
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
// In Startup.ConfigureServices

services.AddDbContext<DbContextA>(options => options.UseSqlite("Data Source=A.db"));
services.AddDbContext<DbContextB>(options => options.UseSqlite("Data Source=B.db"));

services.AddScoped<IResourceRepository<ResourceA>, DbContextARepository<ResourceA>>();
services.AddScoped<IResourceRepository<ResourceB>, DbContextBRepository<ResourceB>>();

services.AddJsonApi(dbContextTypes: new[] {typeof(DbContextA), typeof(DbContextB)});
```
