# Querying Data
### Pagination

Resources can be paginated. 
The following query would set the page size to 10 and get page 2.

```
?page[size]=10&page[number]=2
```

If you would like pagination implemented by default, you can specify the page size
when setting up the services:

```csharp
 services.AddJsonApi<AppDbContext>(
     opt => opt.DefaultPageSize = 10);
```

**Total Record Count**

The total number of records can be added to the document meta by setting it in the options:

```csharp
services.AddJsonApi<AppDbContext>(opt =>
{
    opt.DefaultPageSize = 5;
    opt.IncludeTotalRecordCount = true;
});
```

### Filtering

You can filter resources by attributes using the `filter` query parameter. 
By default, all attributes are filterable.
The filtering strategy we have selected, uses the following form:

```
?filter[attribute]=value
```

For operations other than equality, the query can be prefixed with an operation
identifier):

```
?filter[attribute]=eq:value
?filter[attribute]=lt:value
?filter[attribute]=gt:value
?filter[attribute]=le:value
?filter[attribute]=ge:value
?filter[attribute]=like:value
```

#### Custom Filters

You can customize the filter implementation by overriding the method in the `DefaultEntityRepository` like so:

```csharp
public class MyEntityRepository : DefaultEntityRepository<MyEntity>
{
    public MyEntityRepository(
    	AppDbContext context,
        ILoggerFactory loggerFactory,
        IJsonApiContext jsonApiContext)
    : base(context, loggerFactory, jsonApiContext)
    { }
    
    public override IQueryable<TEntity> Filter(IQueryable<TEntity> entities,  FilterQuery filterQuery)
    {
        // use the base filtering method    
        entities = base.Filter(entities, filterQuery);
	
	// implement custom method
	return ApplyMyCustomFilter(entities, filterQuery);
    }
}
```

### Sorting

Resources can be sorted by an attribute:

```
?sort=attribute // ascending
?sort=-attribute // descending
```

### Meta

Meta objects can be assigned in two ways:
 - Resource meta
 - Request Meta

Resource meta can be defined by implementing `IHasMeta` on the model class:

```csharp
public class Person : Identifiable<int>, IHasMeta
{
    // ...

    public Dictionary<string, object> GetMeta(IJsonApiContext context)
    {
        return new Dictionary<string, object> {
            { "copyright", "Copyright 2015 Example Corp." },
            { "authors", new string[] { "Jared Nance" } }
        };
    }
}
```

Request Meta can be added by injecting a service that implements `IRequestMeta`.
In the event of a key collision, the Request Meta will take precendence. 

### Client Generated Ids

By default, the server will respond with a `403 Forbidden` HTTP Status Code if a `POST` request is
received with a client generated id. However, this can be allowed by setting the `AllowClientGeneratedIds`
flag in the options:

```csharp
services.AddJsonApi<AppDbContext>(opt =>
{
    opt.AllowClientGeneratedIds = true;
    // ..
});
```

### Custom Errors

By default, errors will only contain the properties defined by the internal [Error](https://github.com/Research-Institute/json-api-dotnet-core/blob/master/src/JsonApiDotNetCore/Internal/Error.cs) class. However, you can create your own by inheriting from `Error` and either throwing it in a `JsonApiException` or returning the error from your controller.

```csharp
// custom error definition
public class CustomError : Error {
    public CustomError(string status, string title, string detail, string myProp)
    : base(status, title, detail)
    {
        MyCustomProperty = myProp;
    }
    public string MyCustomProperty { get; set; }
}

// throwing a custom error
public void MyMethod() {
    var error = new CustomError("507", "title", "detail", "custom");
    throw new JsonApiException(error);
}

// returning from controller
[HttpPost]
public override async Task<IActionResult> PostAsync([FromBody] MyEntity entity)
{
    if(_db.IsFull)
        return new ObjectResult(new CustomError("507", "Database is full.", "Theres no more room.", "Sorry."));

    // ...
}
```

### Sparse Fieldsets

We currently support top-level field selection. 
What this means is you can restrict which fields are returned by a query using the `fields` query parameter, but this does not yet apply to included relationships.

- Currently valid:
```http
GET /articles?fields[articles]=title,body HTTP/1.1
Accept: application/vnd.api+json
```

- Not yet supported:
```http
GET /articles?include=author&fields[articles]=title,body&fields[people]=name HTTP/1.1
Accept: application/vnd.api+json
```

## Tests

I am using DotNetCoreDocs to generate sample requests and documentation.

1. To run the tests, start a postgres server and verify the connection properties define in `/test/JsonApiDotNetCoreExampleTests/appsettings.json`
2. `cd ./test/JsonApiDotNetCoreExampleTests`
3. `dotnet test`
4. `cd ./src/JsonApiDotNetCoreExample`
5. `dotnet run`
6. `open http://localhost:5000/docs`
