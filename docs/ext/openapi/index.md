# JSON:API Extension for OpenAPI

This extension facilitates using OpenAPI client generators targeting JSON:API documents.

In JSON:API, a resource object contains the `type` member, which defines the structure of nested [attributes](https://jsonapi.org/format/#document-resource-object-attributes) and [relationships](https://jsonapi.org/format/#document-resource-object-relationships) objects.
While OpenAPI supports such constraints using `allOf` inheritance with a discriminator property for the `data` member,
it provides no way to express that the discriminator recursively applies to nested objects.

This extension addresses that limitation by defining additional discriminator properties to guide code generation tools.

## URI

This extension has the URI `https://www.jsonapi.net/ext/openapi`.
Because code generators often choke on the double quotes in `Accept` and `Content-Type` HTTP header values, a relaxed form is also permitted: `openapi`.

For example, the following `Content-Type` header:

```http
Content-Type: application/vnd.api+json; ext="https://www.jsonapi.net/ext/openapi"
```

is equivalent to:

```http
Content-Type: application/vnd.api+json; ext=openapi
```

To avoid the need for double quotes when multiple extensions are used, the following relaxed form can be used:

```http
Content-Type: application/vnd.api+json; ext=openapi; ext=atomic
```

> [!NOTE]
> The [base specification](https://jsonapi.org/format/#media-type-parameter-rules) *forbids* the use of multiple `ext` parameters
> and *requires* that each extension name must be a URI.
> This extension relaxes both constraints for practical reasons, to workaround bugs in client generators that produce broken code otherwise.

## Namespace

This extension uses the namespace `openapi`.

> [!NOTE]
> JSON:API extensions can only introduce new document members using a reserved namespace as a prefix.

## Document Structure

A document that supports this extension MAY include any of the top-level members allowed by the base specification,
including any members defined in the [Atomic Operations extension](https://jsonapi.org/ext/atomic/).

### Resource Objects

In addition to the members allowed by the base specification, the following member MAY be included
in [attributes](https://jsonapi.org/format/#document-resource-object-attributes) and [relationships](https://jsonapi.org/format/#document-resource-object-relationships) objects:

* `openapi:discriminator` - A string that MUST be identical to the `type` member in the containing [resource object](https://jsonapi.org/format/#document-resource-objects).

Here's how an article (i.e. a resource of type "articles") might appear in a document:

```json
{
  "data": {
    "type": "articles",
    "id": "1",
    "attributes": {
      "openapi:discriminator": "articles",
      "title": "Rails is Omakase"
    },
    "relationships": {
      "openapi:discriminator": "articles",
      "author": {
        "data": { "type": "people", "id": "9" }
      }
    }
  }
}
```

### Atomic Operations

In addition to the members allowed by the [Atomic Operations extension](https://jsonapi.org/ext/atomic/), 
the following member MAY be included in elements of an `atomic:operations` array:

* `openapi:discriminator` - A free-format string to facilitate generation of client code.

For example:

```http
POST /operations HTTP/1.1
Host: example.org
Content-Type: application/vnd.api+json; ext="https://www.jsonapi.net/ext/openapi https://jsonapi.org/ext/atomic"
Accept: application/vnd.api+json; ext="https://www.jsonapi.net/ext/openapi https://jsonapi.org/ext/atomic"

{
  "atomic:operations": [{
    "openapi:discriminator": "add-article",
    "op": "add",
    "data": {
      "type": "articles",
      "attributes": {
        "openapi:discriminator": "articles",
        "title": "JSON API paints my bikeshed!"
      }
    }
  }]
}
```

## Processing

A server MAY ignore the `openapi:discriminator` member in [attributes](https://jsonapi.org/format/#document-resource-object-attributes) and [relationships](https://jsonapi.org/format/#document-resource-object-relationships) objects from incoming requests.
A server SHOULD ignore the `openapi:discriminator` member in elements of an `atomic:operations` array.

A server MUST include the `openapi:discriminator` member in [attributes](https://jsonapi.org/format/#document-resource-object-attributes) and [relationships](https://jsonapi.org/format/#document-resource-object-relationships) objects in outgoing responses.
The member value MUST be the same as the `type` member value of the containing resource object.

A client MAY include the `openapi:discriminator` member in [attributes](https://jsonapi.org/format/#document-resource-object-attributes) and [relationships](https://jsonapi.org/format/#document-resource-object-relationships) objects in outgoing requests.
The member value MUST be the same as the `type` member value of the containing resource object.

A client MAY include the `openapi:discriminator` member in elements of an `atomic:operations` array.

### Processing Errors

A server SHOULD validate that the value of the `openapi:discriminator` member in
[attributes](https://jsonapi.org/format/#document-resource-object-attributes) and [relationships](https://jsonapi.org/format/#document-resource-object-relationships) objects
is identical to the `type` member in the containing resource object. When validation fails, the server MUST respond with a `409 Conflict`
and SHOULD include a document with a top-level `errors` member that contains an error object.
