---
currentMenu: includingRelationships
---

# Including Relationships

JADNC supports [request include params](http://jsonapi-resources.com/v0.9/guide/resources.html#Included-relationships-side-loading-resources) out of the box, for side loading related resources.

Here’s an example from the spec:

```http
GET /articles/1?include=comments HTTP/1.1
Accept: application/vnd.api+json
```

Will get you the following payload:

```json
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
  "included": [{
    "type": "comments",
    "id": "5",
    "attributes": {
      "body": "First!"
    }
  }, {
    "type": "comments",
    "id": "12",
    "attributes": {
      "body": "I like XML better"
    }
  }]
}
```

