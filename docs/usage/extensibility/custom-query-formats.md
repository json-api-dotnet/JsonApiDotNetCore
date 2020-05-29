# Custom Query Formats

For information on the default query string parameter formats, see the documentation for each query method.

In order to customize the query formats, you need to implement the `IQueryParameterParser` interface and inject it.

```c#
services.AddScoped<IQueryParameterParser, FooQueryParameterParser>();
```
