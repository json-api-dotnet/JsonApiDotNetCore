# Creating resources

A single resource can be created by sending a POST request. The next example creates a new article:

```http
POST /articles HTTP/1.1

{
  "data": {
    "type": "articles",
    "attributes": {
      "caption": "A new article!",
      "url": "www.website.com"
    }
  }
}
```

When using client-generated IDs and only attributes from the request have changed, the server returns `204 No Content`.
Otherwise, the server returns `200 OK`, along with the updated resource and its newly assigned ID.

In both cases, a `Location` header is returned that contains the URL to the new resource.

# Creating resources with relationships

It is possible to create a new resource and establish relationships to existing resources in a single request.
The example below creates an article and sets both its owner and tags.

```http
POST /articles HTTP/1.1

{
  "data": {
    "type": "articles",
    "attributes": {
      "caption": "A new article!"
    },
    "relationships": {
      "author": {
        "data": {
          "type": "person",
          "id": "101"
        }
      },
      "tags": {
        "data": [
          {
            "type": "tag",
            "id": "123"
          },
          {
            "type": "tag",
            "id": "456"
          }
        ]
      }
    }
  }
}
```

# Response body

POST requests can be combined with query string parameters that are normally used for reading data, such as `include` and `fields`. For example:

```http
POST /articles?include=owner&fields[owner]=firstName HTTP/1.1

{
  ...
}
```

After the resource has been created on the server, it is re-fetched from the database using the specified query string parameters and returned to the client.
