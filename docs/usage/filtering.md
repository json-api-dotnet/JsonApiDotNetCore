# Filtering

Resources can be filtered by attributes using the `filter` query string parameter.
By default, all attributes are filterable.
The filtering strategy we have selected, uses the following form.

```
?filter[attribute]=value
```

For operations other than equality, the query can be prefixed with an operation identifier.
Examples can be found in the table below.

| Operation                     | Prefix        | Example                                   |
|-------------------------------|---------------|-------------------------------------------|
| Equals                        | `eq`          | `?filter[attribute]=eq:value`             |
| Not Equals                    | `ne`          | `?filter[attribute]=ne:value`             |
| Less Than                     | `lt`          | `?filter[attribute]=lt:10`                |
| Greater Than                  | `gt`          | `?filter[attribute]=gt:10`                |
| Less Than Or Equal To         | `le`          | `?filter[attribute]=le:10`                |
| Greater Than Or Equal To      | `ge`          | `?filter[attribute]=ge:10`                |
| Like (string comparison)      | `like`        | `?filter[attribute]=like:value`           |
| In Set                        | `in`          | `?filter[attribute]=in:value1,value2`     |
| Not In Set                    | `nin`         | `?filter[attribute]=nin:value1,value2`    |
| Is Null                       | `isnull`      | `?filter[attribute]=isnull:`              |
| Is Not Null                   | `isnotnull`   | `?filter[attribute]=isnotnull:`           |

Filters can be combined and will be applied using an AND operator.
The following are equivalent query forms to get articles whose ordinal values are between 1-100.

```http
GET /api/articles?filter[ordinal]=gt:1,lt:100 HTTP/1.1
```
```http
GET /api/articles?filter[ordinal]=gt:1&filter[ordinal]=lt:100 HTTP/1.1
```

Aside from filtering on the resource being requested (top-level), filtering on single-depth related resources that are being included can be done too.

```http
GET /api/articles?include=author&filter[title]=like:marketing&filter[author.lastName]=Smith HTTP/1.1
```

Due to a [limitation](https://github.com/dotnet/efcore/issues/1833) in Entity Framework Core 3.x, filtering does **not** work on nested endpoints:

```http
GET /api/blogs/1/articles?filter[title]=like:new HTTP/1.1
```


## Custom Filters

There are two ways you can add custom filters:

1. Creating a `ResourceDefinition` as [described previously](~/usage/resources/resource-definitions.html#custom-query-filters)
2. Overriding the `DefaultResourceRepository` shown below

```c#
public class AuthorRepository : DefaultResourceRepository<Author>
{
    public AuthorRepository(
        ITargetedFields targetedFields,
        IDbContextResolver contextResolver,
        IResourceGraph resourceGraph,
        IGenericServiceFactory genericServiceFactory,
        IResourceFactory resourceFactory,
        ILoggerFactory loggerFactory)
        : base(targetedFields, contextResolver, resourceGraph, genericServiceFactory, resourceFactory, loggerFactory)
    { }

    public override IQueryable<Author> Filter(IQueryable<Author> authors, FilterQueryContext filterQueryContext)
    {
        // If the filter key is "name" (filter[name]), find authors with matching first or last names.
        // For all other filter keys, use the base method.
        return filterQueryContext.Attribute.Is("name")
            ? authors.Where(author =>
                author.FirstName.Contains(filterQueryContext.Value) ||
                author.LastName.Contains(filterQueryContext.Value))
            : base.Filter(authors, filterQueryContext);
    }
```
