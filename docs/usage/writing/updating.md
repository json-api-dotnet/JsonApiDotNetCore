# Updating resources

## Updating resource attributes

To modify the attributes of a single resource, send a PATCH request. The next example changes the article caption:

```http
POST /articles HTTP/1.1

{
  "data": {
    "type": "articles",
    "id": "1",
    "attributes": {
      "caption": "This has changed"
    }
  }
}
```

This preserves the values of all other unsent attributes and is called a *partial patch*.

When only the attributes that were sent in the request have changed, the server returns `204 No Content`.
But if additional attributes have changed (for example, by a database trigger that refreshes the last-modified date) the server returns `200 OK`, along with all attributes of the updated resource.

## Updating resource relationships

Besides its attributes, the relationships of a resource can be changed using a PATCH request too.
Note that all resources being assigned in a relationship must already exist.

When updating a HasMany relationship, the existing set is replaced by the new set. See below on how to add/remove resources.

The next example replaces both the owner and tags of an article.

```http
PATCH /articles/1 HTTP/1.1

{
  "data": {
    "type": "articles",
    "id": "1",
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

A HasOne relationship can be cleared by setting `data` to `null`, while a HasMany relationship can be cleared by setting it to an empty array.

By combining the examples above, both attributes and relationships can be updated using a single PATCH request.

## Response body

PATCH requests can be combined with query string parameters that are normally used for reading data, such as `include` and `fields[]`. For example:

```http
PATCH /articles/1?include=owner&fields[people]=firstName HTTP/1.1

{
  ...
}
```

After the resource has been updated on the server, it is re-fetched from the database using the specified query string parameters and returned to the client.
Note this only has an effect when `200 OK` is returned.

# Updating relationships

Although relationships can be modified along with resources (as described above), it is also possible to change a single relationship using a relationship URL.
The same rules for clearing the relationship apply. And similar to PATCH on a resource URL, updating a HasMany relationship replaces the existing set.

The next example changes just the owner of an article, by sending a PATCH request to its relationship URL.

```http
PATCH /articles/1/relationships/owner HTTP/1.1

{
  "data": {
    "type": "person",
    "id": "101"
  }
}
```

The server returns `204 No Content` when the update is successful.

## Changing HasMany relationships

The POST and DELETE verbs can be used on HasMany relationship URLs to add or remove resources to/from an existing set without replacing it.

The next example adds another tag to the existing set of tags of an article:

```http
POST /articles/1/relationships/tags HTTP/1.1

{
  "data": [
    {
      "type": "tag",
      "id": "789"
    }
  ]
}
```

Likewise, the next example removes a single tag from the set of tags of an article:

```http
DELETE /articles/1/relationships/tags HTTP/1.1

{
  "data": [
    {
      "type": "tag",
      "id": "789"
    }
  ]
}
```
