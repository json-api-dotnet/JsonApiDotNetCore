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

When using client-generated IDs and all attributes of the created resource are the same as in the request, the server
returns `204 No Content`. Otherwise, the server returns `201 Created`, along with the stored attributes and its newly assigned ID.

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

# Creating resources with compound and collection attributes

_since v6.0_

[Compound attributes](~/usage/resources/attributes.md#compound-attributes) and [collection attributes](~/usage/resources/attributes.md#collection-attributes) are sent as JSON objects and arrays:

```http
POST /contacts HTTP/1.1

{
  "data": {
    "type": "contacts",
    "attributes": {
      "displayName": "Ada",
      "name": { "firstName": "Ada", "lastName": "Lovelace" },
      "phoneNumbers": [
        { "number": "555-0100", "label": "mobile" },
        { "number": "555-0101" }
      ],
      "tags": [ "friend" ]
    }
  }
}
```

- Nested members that are omitted keep the value they get from the constructor of the compound type.
- `null` is accepted for nullable compound and collection attributes. An empty array creates an empty collection.
- Nested members are subject to the same rules as top-level attributes: a member that does not allow `AllowCreate` or is read-only is rejected with HTTP 422, and members that do not exist
  are rejected too, unless `AllowUnknownFieldsInRequestBody` is turned on, in which case they are ignored.
- Public properties of a compound type that do not have `[Attr]` cannot be sent, but can be assigned by your own code,
  for example in a [resource definition](~/usage/extensibility/resource-definitions.md).
- [Model validation](~/usage/advanced/model-state.md#compound-and-collection-attributes) is applied to nested members and collection elements.

Errors point to the offending member using a JSON pointer, with array elements addressed by index. For example, when `number` is omitted from the second phone number:

```json
{
  "errors": [
    {
      "status": "422",
      "title": "Input validation failed.",
      "detail": "The Number field is required.",
      "source": {
        "pointer": "/data/attributes/phoneNumbers/1/number"
      }
    }
  ]
}
```

# Response body

POST requests can be combined with query string parameters that are normally used for reading data, such as `include` and `fields[]`. For example:

```http
POST /articles?include=owner&fields[people]=firstName HTTP/1.1

{
  ...
}
```

> [!NOTE]
> After the resource has been created on the server, it is re-fetched from the database using the specified query string parameters and returned to the client.
> However, the used query string parameters only have an effect when `200 OK` is returned.
