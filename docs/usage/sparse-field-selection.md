# Sparse Field Selection

As an alternative to returning all attributes from a resource, the `fields` query string parameter can be used to select only a subset.
This can be used on the resource being requested (top-level), as well as on single-depth related resources that are being included.

Top-level example:
```http
GET /articles?fields=title,body HTTP/1.1
```

Example for an included relationship:
```http
GET /articles?include=author&fields[author]=name HTTP/1.1
```

Example for both top-level and relationship:
```http
GET /articles?fields=title,body&include=author&fields[author]=name HTTP/1.1
```

Field selection currently does **not** work on nested endpoints:

```http
GET /api/blogs/1/articles?fields=title,body HTTP/1.1
```
