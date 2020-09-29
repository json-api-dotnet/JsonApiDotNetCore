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
