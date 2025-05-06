# Query String Functions

The code [here](https://github.com/json-api-dotnet/JsonApiDotNetCore/tree/master/test/JsonApiDotNetCoreTests/IntegrationTests/QueryStrings/CustomFunctions) shows how to define custom functions that clients can use in JSON:API query string parameters.

- IsUpperCase: Adds the `isUpperCase` function, which can be used in filters on `string` attributes.
  - Returns whether the attribute value is uppercase.
  - Example usage: `GET /blogs/1/posts?filter=and(isUpperCase(caption),not(isUpperCase(url)))`
- StringLength: Adds the `length` function, which can be used in filters and sorts on `string` attributes.
  - Returns the number of characters in the attribute value.
  - Example filter usage: `GET /blogs?filter=greaterThan(length(title),'2')`
  - Example sort usage: `GET /blogs/1/posts?sort=length(caption),-length(url)`
- Sum: Adds the `sum` function, which can be used in filters.
  - Returns the sum of the numeric attribute values in related resources.
  - Example: `GET /blogPosts?filter=greaterThan(sum(comments,numStars),'4')`
- TimeOffset: Adds the `timeOffset` function, which can be used in filters on `DateTime` attributes.
  - Calculates the difference between the attribute value and the current date.
  - A generic resource definition intercepts all filters, rewriting the usage of `timeOffset` into the equivalent filters on the target attribute.
  - Example: `GET /reminders?filter=greaterOrEqual(remindsAt,timeOffset('+0:10:00'))`

The basic pattern to implement a custom function is to:
- Define a custom expression type, which inherits from one of the built-in expression types, such as `FilterExpression` or `FunctionExpression`.
- Inherit from one of the built-in parsers, such as `FilterParser` or `SortParser`, to convert tokens to your custom expression type. Override the `ParseFilter` or `ParseFunction` method.
- Inherit from one of the built-in query clause builders, such as `WhereClauseBuilder` or `OrderClauseBuilder`, to produce a LINQ expression for your custom expression type. Override the `DefaultVisit` method.
