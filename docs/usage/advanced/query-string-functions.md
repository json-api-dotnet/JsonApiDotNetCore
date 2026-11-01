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

## Compound attributes and element scope

_since v6.0_

Field chains are matched against a `FieldChainPattern`, which consists of tokens that stand for kinds of fields:
`O` (to-one relationship), `M` (to-many relationship), `R` (any relationship), `S` (scalar non-collection attribute), `L` (scalar collection),
`C` (compound attribute), `Q` (compound collection), `A` (any attribute) and `F` (any field). A token can be grouped as in `[OC]` and followed by
`?`, `*` or `+`. The built-in patterns are in `BuiltInPatterns`, for example `ToOneOrCompoundChainEndingInScalarAttribute` (`[OC]*S`).
Custom patterns can be created using `FieldChainPattern.Parse("[OC]*A")`.

Inside a custom parser, call `ParseFieldChain(pattern, container)` with the container in scope. In a `FilterParser` subclass, the property
`CurrentScope` tells what the names in the expression are relative to, because a `has` condition changes the scope:

- `AttributeContainerFilterScope`: names are resolved on a resource type or compound type (`Container`), which is the case at the top level
  and inside the condition of `has` on a to-many relationship or a compound collection.
- `ScalarCollectionElementFilterScope`: inside the condition of `has` on a scalar collection, where the element symbol `_` is the only
  operand. Call `TryParseScalarCollectionElement` to parse it. The `ElementType` property tells the type of the elements.

A custom function that should work inside `has` therefore handles both scopes, as the `isUpperCase` sample does:
`has(tags,isUpperCase(_))` and `has(phoneNumbers,isUpperCase(number))`. Use `InScopeOfContainer` and `InScopeOfScalarCollection`
(always disposed) when your own function introduces a nested scope.

Expressions that reference the element symbol are represented as `ScalarCollectionElementValueExpression`.
Sort parsers and `count` take chains that end in a collection (`ToOneOrCompoundChainEndingInCollection`), so custom functions can reuse
those patterns to target compound attributes and collections.
