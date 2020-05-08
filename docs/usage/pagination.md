# Pagination

Resources can be paginated. This query would fetch the second page of 10 articles (articles 11 - 21).

```http
GET /articles?page[size]=10&page[number]=2 HTTP/1.1
```

## Configuring Default Behavior

You can configure the global default behavior as [described previously](~/usage/options.html#pagination).
