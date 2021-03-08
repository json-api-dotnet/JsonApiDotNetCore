# Global Options

Configuration can be applied when adding the services to the dependency injection container.

```c#
public class Startup
{
    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddJsonApi<AppDbContext>(options =>
        {
            // configure the options here
        });
    }
}
```

## Client Generated IDs

By default, the server will respond with a 403 Forbidden HTTP Status Code if a POST request is received with a client-generated ID.

However, this can be allowed by setting the AllowClientGeneratedIds flag in the options:

```c#
options.AllowClientGeneratedIds = true;
```

## Pagination

The default page size used for all resources can be overridden in options (10 by default). To disable paging, set it to `null`.
The maximum page size and number allowed from client requests can be set too (unconstrained by default).
You can also include the total number of resources in each response. Note that when using this feature, it does add some query overhead since we have to also request the total number of resources.

```c#
options.DefaultPageSize = new PageSize(25);
options.MaximumPageSize = new PageSize(100);
options.MaximumPageNumber = new PageNumber(50);
options.IncludeTotalResourceCount = true;
```

## Relative Links

All links are absolute by default. However, you can configure relative links.

```c#
options.UseRelativeLinks = true;
```

```json
{
  "type": "articles",
  "id": "4309",
  "relationships": {
     "author": {
       "links": {
         "self": "/api/v1/articles/4309/relationships/author",
         "related": "/api/v1/articles/4309/author"
       }
     }
  }
}
```

## Unknown Query String Parameters

If you would like to allow unknown query string parameters (parameters not reserved by the JSON:API specification or registered using resource definitions), you can set `AllowUnknownQueryStringParameters = true`. When set to `false` (the default), an HTTP 400 Bad Request is returned for unknown query string parameters.

```c#
options.AllowUnknownQueryStringParameters = true;
```

## Maximum include depth

To limit the maximum depth of nested includes, use `MaximumIncludeDepth`. This is null by default, which means unconstrained. If set and a request exceeds the limit, an HTTP 400 Bad Request is returned.

```c#
options.MaximumIncludeDepth = 1;
```

## Custom Serializer Settings

We use [Newtonsoft.Json](https://www.newtonsoft.com/json) for all serialization needs.
If you want to change the default serializer settings, you can:

```c#
options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
options.SerializerSettings.Converters.Add(new StringEnumConverter());
options.SerializerSettings.Formatting = Formatting.Indented;
```

The default naming convention (as used in the routes and resource/attribute/relationship names) is also determined here, and can be changed (default is camel-case):

```c#
options.SerializerSettings.ContractResolver = new DefaultContractResolver
{
    NamingStrategy = new KebabCaseNamingStrategy()
};
```

Because we copy resource properties into an intermediate object before serialization, Newtonsoft.Json annotations on properties are ignored.


## Enable ModelState Validation

If you would like to use ASP.NET Core ModelState validation into your controllers when creating / updating resources, set `ValidateModelState` to `true`. By default, no model validation is performed.

```c#
options.ValidateModelState = true;
```

```c#
public class Person : Identifiable
{
    [Attr]
    [Required]
    [MinLength(3)]
    public string FirstName { get; set; }
}
```
