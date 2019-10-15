# Query Parameter Services

This article describes 
1. how URL query parameters are currently processed internally 
2. how to customize the behaviour of existing query parameters
3. how to register your own

##  1. Internal usage

Below is a list of the query parameters that are supported. Each supported query parameter has it's own dedicated service.

| Query Parameter Service | Occurence in URL               | Domain                                                |
|-------------------------|--------------------------------|-------------------------------------------------------|
| `IFilterService`         | `?filter[article.title]=title`    | filtering the resultset                               |
| `IIncludeService`         | `?include=article.author`        | including related data                                |
| `IPageService`            | `?page[size]=10&page[number]=3`  | pagination of the resultset                           |
| `ISortService`            | `?sort=-title`                   | sorting the resultset                                 |
| `ISparseFieldsService`    | `?fields[article]=title,summary` | sparse field selection                                |
| `IOmitDefaultService`     | `?omitDefault=true`              | omitting default values from the serialization result |
| `IOmitNullService`        | `?omitNull=false`                | omitting null values from the serialization result    |


These services are responsible for parsing the value from the URL by gathering relevant (meta)data and performing validations as required by JsonApiDotNetCore down the pipeline. For example, the `IIncludeService` is responsible for checking if `article.author` is a valid relationship chain, and pre-processes the chain into a `List<RelationshipAttibute>` so that the rest of the framework can process it easier.

Each of these services implement the `IQueryParameterService` interface, which exposes:
* a `Name` property that is used internally to match the URL query parameter to the service.
	`IIncludeService.Name` returns `include`, which will match `include=article.author`
* a `Parse` method that is called internally in the middleware to process the url query parameters.


```c#
public interface IQueryParameterService
{
    /// <summary>
    /// Parses the value of the query parameter. Invoked in the middleware.
    /// </summary>
    /// <param name="queryParameter">the value of the query parameter as retrieved from the url</param>
    void Parse(KeyValuePair<string, StringValues> queryParameter);
    /// <summary>
    /// The name of the query parameter as matched in the URL query string.
    /// </summary>
    string Name { get; }
}
``` 

The piece of internals that is responsible for calling the `Parse` method is the `IQueryParameterParser` service (formally known as `QueryParser`). This service injects every registered implementation of `IQueryParameterService` and calls the parse method with the appropiate part of the url querystring.


## 2. Customizing behaviour
You can register your own implementation of every service interface in the table above. As an example, we may want to add additional support for `page[index]=3` next to `page[number]=3` ("number" replaced with "index"). This could be achieved as follows

```c#
// CustomPageService.cs
public class CustomPageService : PageService
{
    public override void Parse(KeyValuePair<string, StringValues> queryParameter)
    {	
    	var key = queryParameter.Key.Replace("index", "number");
    	queryParameter = KeyValuePair<string, StringValues>(key, queryParameter.Value);
    	base.Parse(queryParameter)
    }
}

// Startup.cs
services.AddScoped<IPageService, CustomPageService>();
```

## 3. Registering new services
You may also define an entirely new custom query parameter. For example, we want to trigger a `HTTP 418 I'm a teapot` if a client includes a `?teapot=true` query parameter. This could be implemented as follows:


```c#
// ITeapotService.cs
public interface ITeapotService
{
	// Interface containing the "business logic" of the query parameter service, 
	// in a way useful to  your application
	bool ShouldThrowTeapot { get; }
}

// TeapotService.cs
public class TeapotService : IQueryParameterService, ITeapotService
{	// ^^^ must inherit the IQueryParameterService interface
	pubic bool ShouldThrowTeapot { get; }

	public string Name => "teapot";

    public override void Parse(KeyValuePair<string, StringValues> queryParameter)
    {	
    	if(bool.Parse(queryParameter.Value, out bool config))
    		ShouldThrowTeapot = true;
    }
}

// Startup.cs
services.AddScoped<ITeapotService, TeapotService>(); // exposes the parsed query parameter to your application
services.AddScoped<IQueryParameterService, TeapotService>(); // ensures that the associated query parameter service will be parsed internally by JADNC.
```

Things to pay attention to:
* The teapot service must be registered as an implementation of `IQueryParameterService` to be processed internally in the middleware
* Any other (business) logic is exposed on ITeapotService for usage in your application.


Now, we could access the custom query parameter service anywhere in our application to trigger a 418. Let's use the resource hooks to include this piece of business logic
```c#
public class TodoResource : ResourceDefinition<TodoItem>
{
	private readonly ITeapotService _teapotService;

    public TodoResource(IResourceGraph graph, ITeapotService teapotService) : base(graph) 
    { 
    	_teapotService = teapotService
    }

    public override void BeforeRead(ResourcePipeline pipeline, bool isIncluded = false, string stringId = null)
    {
    	if (teapotService.ShouldThrowTeapot)
    		throw new JsonApiException(418, "This is caused by the usage of teapot=true.")
    }

}
```