# Sparse Fieldset Selection

As an alternative to returning all attributes from a resource, the `fields` query string parameter can be used to select only a subset.
This can be used on the resource being requested, as well as nested endpoints and/or included resources.

Top-level example:
```http
GET /articles?fields=title,body HTTP/1.1
```

Nested endpoint example:
```http
GET /api/blogs/1/articles?fields=title,body HTTP/1.1
```

Example for an included HasOne relationship:
```http
GET /articles?include=author&fields[author]=name HTTP/1.1
```

Example for an included HasMany relationship:
```http
GET /articles?include=revisions&fields[revisions]=publishTime HTTP/1.1
```

Example for both top-level and relationship:
```http
GET /articles?include=author&fields=title,body&fields[author]=name HTTP/1.1
```

## Overriding

As a developer, you can force to include and/or exclude specific fields as [described previously](~/usage/resources/resource-definitions.md).
