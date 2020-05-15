# Sorting

Resources can be sorted by one or more attributes.
The default sort order is ascending.
To sort descending, prepend the sort key with a minus (-) sign.

## Ascending

```http
GET /api/articles?sort=author HTTP/1.1
```

## Descending

```http
GET /api/articles?sort=-author HTTP/1.1
```

## Multiple attributes

```http
GET /api/articles?sort=author,-pageCount HTTP/1.1
```

## Limitations

Sorting currently does **not** work on nested endpoints:

```http
GET /api/blogs/1/articles?sort=title HTTP/1.1
```

## Default Sort

See the topic on [Resource Definitions](~/usage/resources/resource-definitions)
for defining the default sort behavior.
