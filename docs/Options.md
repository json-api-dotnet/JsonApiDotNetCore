---
currentMenu: options
---

# Global Options

## Client Generated Ids

By default, the server will respond with a `403 Forbidden` HTTP Status Code if a `POST` request is
received with a client generated id. However, this can be allowed by setting the `AllowClientGeneratedIds`
flag in the options:

```csharp
public IServiceProvider ConfigureServices(IServiceCollection services) {
    services.AddJsonApi<AppDbContext>(
        opt => opt.AllowClientGeneratedIds = true);
    // ...
}
```

## Pagination

If you would like pagination implemented by default, you can specify the page size
when setting up the services:

```csharp
public IServiceProvider ConfigureServices(IServiceCollection services) {
    services.AddJsonApi<AppDbContext>(
        opt => opt.DefaultPageSize = 10);
    // ...
}
```

### Total Record Count

The total number of records can be added to the document meta by setting it in the options:

```csharp
public IServiceProvider ConfigureServices(IServiceCollection services) {
    services.AddJsonApi<AppDbContext>(opt =>
    {
        opt.DefaultPageSize = 5;
        opt.IncludeTotalRecordCount = true;
    });
    // ...
}
```

## Relative Links

All links are absolute by default. However, you can configure relative links:

```csharp
public IServiceProvider ConfigureServices(IServiceCollection services) {
    services.AddJsonApi<AppDbContext>(
        opt => opt.RelativeLinks = true);
    // ...
}
```


```http
GET /api/v1/articles/4309 HTTP/1.1
Accept: application/vnd.api+json
```

```json
{
    "type": "articles",
    "id": "4309",
    "attributes": {
        "name": "Voluptas iure est molestias."
    },
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

If you would like to use custom query params (parameters not reserved by the json:api specification), you can set `AllowCustomQueryParameters = true`. The default behavior is to return an `HTTP 400 Bad Request` for unknown query parameters.

```csharp
public IServiceProvider ConfigureServices(IServiceCollection services) {
    services.AddJsonApi<AppDbContext>(
        opt => opt.AllowCustomQueryParameters = true);
    // ...
}
```

## Custom Serializer Settings

We use Json.Net for all serialization needs. If you want to change the default serializer settings, you can:

```csharp
public IServiceProvider ConfigureServices(IServiceCollection services) {
    services.AddJsonApi<AppDbContext>(
        opt => opt.SerializerSettings = new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new DasherizedResolver()
        });
    // ...
}
```