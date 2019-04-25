# Sorting

Resources can be sorted by an attribute. 
The default sort order is ascending. 
To sort descending, prepend the sort key with a minus (-) sign.

## Ascending

```http
GET /api/articles?sort=author HTTP/1.1
Accept: application/vnd.api+json
```

## Descending

```http
GET /api/articles?sort=-author HTTP/1.1
Accept: application/vnd.api+json
```

## Default Sort

See the topic on [Resource Definitions](~/usage/resources/resource-definitions)
for defining the default sort behavior.
