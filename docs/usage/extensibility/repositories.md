# Entity Repositories

If you want to use EF, but need additional data access logic (such as authorization), you can implement custom methods for accessing the data by creating an implementation of IEntityRepository<Entity, TId>. If you only need minor changes you can override the methods defined in DefaultEntityRepository<TEntity, TId>.

The repository should then be add to the service collection in Startup.cs.

```c#
public IServiceProvider ConfigureServices(IServiceCollection services) 
{
    services.AddScoped<IEntityRepository<Article>, AuthorizedArticleRepository>();
    // ...
}
```

A sample implementation that performs data authorization might look like this.

All of the methods in the DefaultEntityRepository will use the Get() method to get the DbSet<TEntity> so this is a good method to apply scoped filters such as user or tenant authorization.

```c#
public class AuthorizedArticleRepository 
  : DefaultEntityRepository<Article>
{
    private readonly IAuthenticationService _authenticationService;

    public AuthorizedArticleRepository(
        ILoggerFactory loggerFactory,
        IJsonApiContext jsonApiContext,
        IDbContextResolver contextResolver,
        IAuthenticationService authenticationService)
    : base(loggerFactory, jsonApiContext, contextResolver)
    {
        _authenticationService = authenticationService;
    }

    public override IQueryable<MyEntity> Get() 
        => base.Get()
            .Where(e => 
                e.UserId == _authenticationService.UserId
            );
}
```

## Multiple DbContexts

If you need to use multiple EF DbContext, first add each DbContext to the ContextGraphBuilder.

Then, create an implementation of IDbContextResolver for each context.

Register each of the new IDbContextResolver implementations in the Startup.

You can then create a general repository for each context and inject it per resource type. This example shows a single DbContextARepository for all entities that are members of DbContextA.

Then inject the repository for the correct entity, in this case Foo is a member of DbContextA.

```c#
// Startup.cs
services.AddJsonApi(options => {
  options.BuildContextGraph((builder) =>
  {
      // Add both contexts using the builder
      builder.AddDbContext<DbContextA>();
      builder.AddDbContext<DbContextB>();
  });
}, mvcBuilder);


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


public class DbContextARepository<TEntity> 
: DefaultEntityRepository<TEntity> where TEntity : class, IIdentifiable<T>
{
  public DbContextARepository(
      ILoggerFactory loggerFactory,
      IJsonApiContext jsonApiContext,
      DbContextAResolver contextResolver)
  : base(loggerFactory, jsonApiContext, contextResolver)
  { }
}


// Startup.cs
services.AddScoped<IEntityRepository<Foo>, DbContextARepository<Foo>>();
```