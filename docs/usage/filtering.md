# Filtering

Resources can be filtered by attributes using the `filter` query parameter. 
By default, all attributes are filterable. 
The filtering strategy we have selected, uses the following form.

```
?filter[attribute]=value
```

For operations other than equality, the query can be prefixed with an operation identifier.
Examples can be found in the table below.

| Operation                     | Prefix         | Example                                  |
|-------------------------------|---------------|------------------------------------------|
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
Accept: application/vnd.api+json
```
```http
GET /api/articles?filter[ordinal]=gt:1&filter[ordinal]=lt:100 HTTP/1.1
Accept: application/vnd.api+json
```

## Custom Filters

There are two ways you can add custom filters:

1. Creating a `ResourceDefinition` as [described previously](~/usage/resources/resource-definitions.html#custom-query-filters)
2. Overriding the `DefaultEntityRepository` shown below

```c#
public class AuthorRepository : DefaultEntityRepository<Author>
{
  public AuthorRepository(
    AppDbContext context,
    ILoggerFactory loggerFactory,
    IJsonApiContext jsonApiContext)
  : base(context, loggerFactory, jsonApiContext)
  { }

  public override IQueryable<TEntity> Filter(
      IQueryable<TEntity> authors, 
      FilterQuery filterQuery)
        // if the filter key is "query" (filter[query]), 
        // find Authors with matching first or last names
        // for all other filter keys, use the base method
        => filter.Attribute.Is("query")
                    ? authors.Where(a => 
                        a.First.Contains(filter.Value)
                        || a.Last.Contains(filter.Value))
                    : base.Filter(authors, filter);
}
```

