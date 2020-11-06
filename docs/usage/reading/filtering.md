# Filtering

_since v4.0_

Resources can be filtered by attributes using the `filter` query string parameter.
By default, all attributes are filterable.
The filtering strategy we have selected, uses the following form.

```
?filter=expression
```

Expressions are composed using the following functions:

| Operation                     | Function           | Example                                               |
|-------------------------------|--------------------|-------------------------------------------------------|
| Equality                      | `equals`           | `?filter=equals(lastName,'Smith')`                    |
| Less than                     | `lessThan`         | `?filter=lessThan(age,'25')`                          |
| Less than or equal to         | `lessOrEqual`      | `?filter=lessOrEqual(lastModified,'2001-01-01')`      |
| Greater than                  | `greaterThan`      | `?filter=greaterThan(duration,'6:12:14')`             |
| Greater than or equal to      | `greaterOrEqual`   | `?filter=greaterOrEqual(percentage,'33.33')`          |
| Contains text                 | `contains`         | `?filter=contains(description,'cooking')`             |
| Starts with text              | `startsWith`       | `?filter=startsWith(description,'The')`               |
| Ends with text                | `endsWith`         | `?filter=endsWith(description,'End')`                 |
| Equals one value from set     | `any`              | `?filter=any(chapter,'Intro','Summary','Conclusion')` |
| Collection contains items     | `has`              | `?filter=has(articles)`                               |
| Negation                      | `not`              | `?filter=not(equals(lastName,null))`                  |
| Conditional logical OR        | `or`               | `?filter=or(has(orders),has(invoices))`               |
| Conditional logical AND       | `and`              | `?filter=and(has(orders),has(invoices))`              |

Comparison operators compare an attribute against a constant value (between quotes), null or another attribute:

```http
GET /users?filter=equals(displayName,'Brian O''Connor') HTTP/1.1
```
```http
GET /users?filter=equals(displayName,null) HTTP/1.1
```
```http
GET /users?filter=equals(displayName,lastName) HTTP/1.1
```

Comparison operators can be combined with the `count` function, which acts on HasMany relationships:

```http
GET /blogs?filter=lessThan(count(owner.articles),'10') HTTP/1.1
```
```http
GET /customers?filter=greaterThan(count(orders),count(invoices)) HTTP/1.1
```

When filters are used multiple times on the same resource, they are combined using an OR operator.
The next request returns all customers that have orders -or- whose last name is Smith.

```http
GET /customers?filter=has(orders)&filter=equals(lastName,'Smith') HTTP/1.1
```

Aside from filtering on the resource being requested (which would be blogs in /blogs and articles in /blogs/1/articles), 
filtering on included collections can be done using bracket notation:

```http
GET /articles?include=author,tags&filter=equals(author.lastName,'Smith')&filter[tags]=contains(label,'tech','design') HTTP/1.1
```

In the above request, the first filter is applied on the collection of articles, while the second one is applied on the nested collection of tags.

Putting it all together, you can build quite complex filters, such as:

```http
GET /blogs?include=owner.articles.revisions&filter=and(or(equals(title,'Technology'),has(owner.articles)),not(equals(owner.lastName,null)))&filter[owner.articles]=equals(caption,'Two')&filter[owner.articles.revisions]=greaterThan(publishTime,'2005-05-05') HTTP/1.1
```

# Legacy filters

The next section describes how filtering worked in versions prior to v4.0. They are always applied on the set of resources being requested (no nesting).
Legacy filters use the following form.

```
?filter[attribute]=value
```

For operations other than equality, the query can be prefixed with an operation identifier.
Examples can be found in the table below.

| Operation                     | Prefix           | Example                                         | Equivalent form in v4.0                               |
|-------------------------------|------------------|-------------------------------------------------|-------------------------------------------------------|
| Equality                      | `eq`             | `?filter[lastName]=eq:Smith`                    | `?filter=equals(lastName,'Smith')`                    |
| Non-equality                  | `ne`             | `?filter[lastName]=ne:Smith`                    | `?filter=not(equals(lastName,'Smith'))`               |
| Less than                     | `lt`             | `?filter[age]=lt:25`                            | `?filter=lessThan(age,'25')`                          |
| Less than or equal to         | `le`             | `?filter[lastModified]=le:2001-01-01`           | `?filter=lessOrEqual(lastModified,'2001-01-01')`      |
| Greater than                  | `gt`             | `?filter[duration]=gt:6:12:14`                  | `?filter=greaterThan(duration,'6:12:14')`             |
| Greater than or equal to      | `ge`             | `?filter[percentage]=ge:33.33`                  | `?filter=greaterOrEqual(percentage,'33.33')`          |
| Contains text                 | `like`           | `?filter[description]=like:cooking`             | `?filter=contains(description,'cooking')`             |
| Equals one value from set     | `in`             | `?filter[chapter]=in:Intro,Summary,Conclusion`  | `?filter=any(chapter,'Intro','Summary','Conclusion')` |
| Equals none from set          | `nin`            | `?filter[chapter]=nin:one,two,three`            | `?filter=not(any(chapter,'one','two','three'))`       |
| Equal to null                 | `isnull`         | `?filter[lastName]=isnull:`                     | `?filter=equals(lastName,null)`                       |
| Not equal to null             | `isnotnull`      | `?filter[lastName]=isnotnull:`                  | `?filter=not(equals(lastName,null))`                  |

Filters can be combined and will be applied using an OR operator. This used to be AND in versions prior to v4.0.

Attributes to filter on can optionally be prefixed with a HasOne relationship, for example:

```http
GET /api/articles?include=author&filter[caption]=like:marketing&filter[author.lastName]=Smith HTTP/1.1
```

Legacy filter notation can still be used in v4.0 by setting `options.EnableLegacyFilterNotation` to `true`.
If you want to use the new filter notation in that case, prefix the parameter value with `expr:`, for example:

```http
GET /articles?filter[caption]=tech&filter=expr:equals(caption,'cooking')) HTTP/1.1
```

## Custom Filters

There are multiple ways you can add custom filters:

1. Implementing `IResourceDefinition.OnApplyFilter` (see [here](~/usage/resources/resource-definitions.md#exclude-soft-deleted-resources)) and inject `IRequestQueryStringAccessor`, which works at all depths, but filter operations are constrained to what `FilterExpression` provides
2. Implementing `IResourceDefinition.OnRegisterQueryableHandlersForQueryStringParameters` as [described previously](~/usage/resources/resource-definitions.md#custom-query-string-parameters), which enables the full range of `IQueryable<T>` functionality, but only works on primary endpoints
3. Add an implementation of `IQueryConstraintProvider` to supply additional `FilterExpression`s, which are combined with existing filters using AND operator
4. Override `EntityFrameworkCoreRepository.ApplyQueryLayer` to adapt the `IQueryable<T>` expression just before execution
5. Take a deep dive and plug into reader/parser/tokenizer/visitor/builder for adding additional general-purpose filter operators
