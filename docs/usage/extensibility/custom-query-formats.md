# Custom Query Formats

For information on the default query parameter formats, see the documentation for each query method.

In order to customize the query formats, you need to implement the `IQueryParser` interface and inject it.

```c#
services.AddScoped<IQueryParser, FooQueryParser>();
```