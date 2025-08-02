# Pagination

Resources can be paginated. This request would fetch the second page of 10 articles (articles 11 - 20).

```http
GET /articles?page[size]=10&page[number]=2 HTTP/1.1
```

Pagination can be used on secondary endpoints, such as:

```http
GET /blogs/1/articles?page[number]=2 HTTP/1.1
```

and on included resources, for example:

```http
GET /api/blogs/1/articles?include=revisions&page[size]=10,revisions:5&page[number]=2,revisions:3 HTTP/1.1
```

> [!NOTE]
> For optimal performance, pagination links and total meta are not returned for *included* to-many relationships.
> See [here](https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/1738) for details.

## Configuring Default Behavior

You can configure the global default behavior as described [here](~/usage/options.md#pagination).

> [!TIP]
> Since v5.8, pagination can be [turned off per relationship](~/usage/resources/relationships.md#disable-pagination).
