# Including Relationships

JsonApiDotNetCore supports [request include params](http://jsonapi.org/format/#fetching-includes) out of the box,
for side-loading related resources.

```http
GET /articles/1?include=comments HTTP/1.1

{
  "data": {
    "type": "articles",
    "id": "1",
    "attributes": {
      "title": "JSON API paints my bikeshed!"
    },
    "relationships": {
      "comments": {
        "links": {
          "self": "http://example.com/articles/1/relationships/comments",
          "related": "http://example.com/articles/1/comments"
        },
        "data": [
          { "type": "comments", "id": "5" },
          { "type": "comments", "id": "12" }
        ]
      }
    }
  },
  "included": [
    {
      "type": "comments",
      "id": "5",
      "attributes": {
        "body": "First!"
      }
    },
    {
      "type": "comments",
      "id": "12",
      "attributes": {
        "body": "I like XML better"
      }
    }
  ]
}
```

## Nested Inclusions

_since v3_

JsonApiDotNetCore also supports nested inclusions.
This allows you to include data across relationships by using a period-delimited relationship path, for example:

```http
GET /api/articles?include=author.livingAddress.country
```

which is equivalent to:

```http
GET /api/articles?include=author&include=author.livingAddress&include=author.livingAddress.country
```

This can be used on nested endpoints too:

```http
GET /api/blogs/1/articles?include=author.livingAddress.country
```
