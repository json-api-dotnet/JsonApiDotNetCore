# Sorting

Resources can be sorted by one or more attributes in ascending or descending order. The default is ascending by ID.

## Ascending

```http
GET /api/articles?sort=author HTTP/1.1
```

## Descending

To sort descending, prepend the attribute with a minus (-) sign.

```http
GET /api/articles?sort=-author HTTP/1.1
```

## Multiple attributes

Multiple attributes are separated by a comma.

```http
GET /api/articles?sort=author,-pageCount HTTP/1.1
```

## Count

To sort on the number of nested resources, use the `count` function.

```http
GET /api/blogs?sort=count(articles) HTTP/1.1
```

This sorts the list of blogs by their number of articles.

## Nesting

Sorting can be used on nested endpoints, such as:

```http
GET /api/blogs/1/articles?sort=caption HTTP/1.1
```

and on included resources, for example:

```http
GET /api/blogs/1/articles?include=revisions&sort=caption&sort[revisions]=publishTime HTTP/1.1
```

This sorts the list of blogs by their captions and included revisions by their publication time.

## Default Sort

See the topic on [Resource Definitions](~/usage/resources/resource-definitions.md)
for overriding the default sort behavior.
