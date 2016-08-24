# JSON API .Net Core

JSON API Spec Conformance: **Non Conforming**

Go [here](https://github.com/Research-Institute/json-api-dotnet-core/wiki/Request-Examples) to see examples of HTTP requests and responses 

## Usage

- Configure the service:

```
services.AddDbContext<ApplicationDbContext>(options =>
  options.UseNpgsql(Configuration["Data:ConnectionString"]),
  ServiceLifetime.Transient);

services.AddJsonApi(config => {
  config.UseContext<ApplicationDbContext>();
  config.SetDefaultNamespace("api/v1");
});
```

- Add middleware:

```
app.UseJsonApi();
```

## References
[JsonApi Specification](http://jsonapi.org/)

## Current Assumptions

- Using Entity Framework
- All entities in the specified context should have controllers
- All entities are served from the same namespace (i.e. 'api/v1')
