# Global Options

Configuration can be applied when adding the services to the DI container.

```c#
public IServiceProvider ConfigureServices(IServiceCollection services) {
    services.AddJsonApi<AppDbContext>(options => {
        // configure the options here
    });
}
```

## Client Generated Ids

By default, the server will respond with a 403 Forbidden HTTP Status Code if a POST request is received with a client generated id.

However, this can be allowed by setting the AllowClientGeneratedIds flag in the options

```c#
services.AddJsonApi<AppDbContext>(options => {
    options.AllowClientGeneratedIds = true;
});
```

## Pagination

If you would like pagination implemented for all resources, you can specify a default page size.

You can also include the total number of records in each request. Note that when using this feature, it does add some query overhead since we have to also request the total number of records.

```c#
services.AddJsonApi<AppDbContext>(options => {
    options.DefaultPageSize = 10;
});
```

## Relative Links

All links are absolute by default. However, you can configure relative links.

```c#
services.AddJsonApi<AppDbContext>(options => {
    options.RelativeLinks = true;
});
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

## Custom Query Parameters

If you would like to use custom query params (parameters not reserved by the json:api specification), you can set AllowCustomQueryParameters = true. The default behavior is to return an HTTP 400 Bad Request for unknown query parameters.

```c#
services.AddJsonApi<AppDbContext>(options => {
    options.AllowCustomQueryParameters = true;
});
```

## Custom Serializer Settings

We use Newtonsoft.Json for all serialization needs.
If you want to change the default serializer settings, you can:

```c#
options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
options.SerializerSettings.ContractResolver = new DasherizedResolver();
```
