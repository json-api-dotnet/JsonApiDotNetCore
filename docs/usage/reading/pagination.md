# Pagination

Resources can be paginated. This request would fetch the second page of 10 articles (articles 11 - 20).

```http
GET /articles?page[size]=10&page[number]=2 HTTP/1.1
```

## Nesting

Pagination can be used on nested endpoints, such as:

```http
GET /blogs/1/articles?page[number]=2 HTTP/1.1
```

and on included resources, for example:

```http
GET /api/blogs/1/articles?include=revisions&page[size]=10,revisions:5&page[number]=2,revisions:3 HTTP/1.1
```

## Configuring Default Behavior

You can configure the global default behavior as [described previously](~/usage/options.md#pagination).
