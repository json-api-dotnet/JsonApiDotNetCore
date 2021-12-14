# Resource Repositories

If you want to use a data access technology other than Entity Framework Core, you can create an implementation of `IResourceRepository<TResource, TId>`.
If you only need minor changes you can override the methods defined in `EntityFrameworkCoreRepository<TResource, TId>`.

```c#
// Program.cs
builder.Services.AddScoped<IResourceRepository<Article, int>, ArticleRepository>();
builder.Services.AddScoped<IResourceReadRepository<Article, int>, ArticleRepository>();
builder.Services.AddScoped<IResourceWriteRepository<Article, int>, ArticleRepository>();
```

In v4.0 we introduced an extension method that you can use to register a resource repository on all of its JsonApiDotNetCore interfaces.
This is helpful when you implement (a subset of) the resource interfaces and want to register them all in one go.

**Note:** If you're using [auto-discovery](~/usage/resource-graph.md#auto-discovery), this happens automatically.

```c#
// Program.cs
builder.Services.AddResourceRepository<ArticleRepository>();
```

A sample implementation that performs authorization might look like this.

All of the methods in EntityFrameworkCoreRepository will use the `GetAll()` method to get the `DbSet<TResource>`, so this is a good method to apply filters such as user or tenant authorization.

```c#
public class ArticleRepository : EntityFrameworkCoreRepository<Article, int>
{
    private readonly IAuthenticationService _authenticationService;

    public ArticleRepository(IAuthenticationService authenticationService,
        ITargetedFields targetedFields, IDbContextResolver dbContextResolver,
        IResourceGraph resourceGraph, IResourceFactory resourceFactory,
        IEnumerable<IQueryConstraintProvider> constraintProviders,
        ILoggerFactory loggerFactory,
        IResourceDefinitionAccessor resourceDefinitionAccessor)
        : base(targetedFields, dbContextResolver, resourceGraph, resourceFactory,
            constraintProviders, loggerFactory, resourceDefinitionAccessor)
    {
        _authenticationService = authenticationService;
    }

    public override IQueryable<Article> GetAll(CancellationToken cancellationToken)
    {
        return base.GetAll(cancellationToken)
            .Where(article => article.UserId == _authenticationService.UserId);
    }
}
```

## Multiple DbContexts

If you need to use multiple Entity Framework Core DbContexts, first create a repository for each context and inject its typed resolver.
This example shows a single `DbContextARepository` for all entities that are members of `DbContextA`.

```c#
public class DbContextARepository<TResource, TId>
    : EntityFrameworkCoreRepository<TResource, TId>
    where TResource : class, IIdentifiable<TId>
{
    public DbContextARepository(ITargetedFields targetedFields,
        DbContextResolver<DbContextA> dbContextResolver,
    //  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        IResourceGraph resourceGraph, IResourceFactory resourceFactory,
        IEnumerable<IQueryConstraintProvider> constraintProviders,
        ILoggerFactory loggerFactory, IResourceDefinitionAccessor resourceDefinitionAccessor)
        : base(targetedFields, dbContextResolver, resourceGraph, resourceFactory,
            constraintProviders, loggerFactory, resourceDefinitionAccessor)
    {
    }
}
```

Then register the added types and use the non-generic overload of `AddJsonApi` to add their resources to the graph.

```c#
// Program.cs
builder.Services.AddDbContext<DbContextA>(options => options.UseSqlite("Data Source=A.db"));
builder.Services.AddDbContext<DbContextB>(options => options.UseSqlite("Data Source=B.db"));

builder.Services.AddJsonApi(dbContextTypes: new[] { typeof(DbContextA), typeof(DbContextB) });

builder.Services.AddScoped<IResourceRepository<ResourceA>, DbContextARepository<ResourceA>>();
builder.Services.AddScoped<IResourceRepository<ResourceB>, DbContextBRepository<ResourceB>>();
```
