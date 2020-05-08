# Entity Repositories

If you want to use Entity Framework Core, but need additional data access logic (such as authorization), you can implement custom methods for accessing the data by creating an implementation of IResourceRepository<TResource, TId>. If you only need minor changes you can override the methods defined in DefaultResourceRepository<TResource, TId>.

The repository should then be added to the service collection in Startup.cs.

```c#
public void ConfigureServices(IServiceCollection services) 
{
    services.AddScoped<IResourceRepository<Article>, AuthorizedArticleRepository>();
    // ...
}
```

A sample implementation that performs data authorization might look like this.

All of the methods in the DefaultResourceRepository will use the Get() method to get the DbSet<TEntity>, so this is a good method to apply scoped filters such as user or tenant authorization.

```c#
public class AuthorizedArticleRepository : DefaultResourceRepository<Article>
{
    private readonly IAuthenticationService _authenticationService;

    public AuthorizedArticleRepository(
        IAuthenticationService authenticationService,
        ITargetedFields targetedFields,
        IDbContextResolver contextResolver,
        IResourceGraph resourceGraph,
        IGenericServiceFactory genericServiceFactory,
        IResourceFactory resourceFactory,
        ILoggerFactory loggerFactory)
        : base(targetedFields, contextResolver, resourceGraph, genericServiceFactory, resourceFactory, loggerFactory)
    {
        _authenticationService = authenticationService;
    }

    public override IQueryable<Article> Get()
    {
        return base.Get().Where(article => article.UserId == _authenticationService.UserId);
    }
}
```

## Multiple DbContexts

If you need to use multiple Entity Framework Core DbContexts, first add each DbContext to the ResourceGraphBuilder.

Then, create an implementation of IDbContextResolver for each context.

Register each of the new IDbContextResolver implementations in Startup.cs.

You can then create a general repository for each context and inject it per resource type. This example shows a single DbContextARepository for all entities that are members of DbContextA.

Then inject the repository for the correct entity, in this case Foo is a member of DbContextA.

```c#
// Startup.cs
services.AddJsonApi(resources: builder =>
{
    // Add both contexts using the builder
    builder.AddDbContext<DbContextA>();
    builder.AddDbContext<DbContextB>();
});

public class DbContextAResolver : IDbContextResolver
{
    private readonly DbContextA _context;

    public DbContextAResolver(DbContextA context)
    {
        _context = context;
    }

    public DbContext GetContext() => _context;
}


// Startup.cs
services.AddScoped<DbContextAResolver>();
services.AddScoped<DbContextBResolver>();


public class DbContextARepository<TResource> : DefaultResourceRepository<TResource>
    where TResource : class, IIdentifiable<int>
{
    public DbContextARepository(
        ITargetedFields targetedFields,
        DbContextAResolver contextResolver,
        IResourceGraph resourceGraph,
        IGenericServiceFactory genericServiceFactory,
        IResourceFactory resourceFactory,
        ILoggerFactory loggerFactory)
        : base(targetedFields, contextResolver, resourceGraph, genericServiceFactory, resourceFactory, loggerFactory)
    { }
}


// Startup.cs
services.AddScoped<IResourceRepository<Foo>, DbContextARepository<Foo>>();
```