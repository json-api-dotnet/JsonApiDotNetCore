# Attributes

If you want an attribute on your model to be publicly available, add the `AttrAttribute`.

```c#
#nullable enable

public class Person : Identifiable<long>
{
    [Attr]
    public string? FirstName { get; set; }

    [Attr]
    public string LastName { get; set; } = null!;
}
```

## Name

There are two ways the exposed attribute name is determined:

1. Using the configured [naming convention](~/usage/options.md#customize-serializer-options).

2. Individually using the attribute's constructor.
```c#
#nullable enable
public class Person : Identifiable<long>
{
    [Attr(PublicName = "first-name")]
    public string? FirstName { get; set; }
}
```

The same rules apply to the members of [compound attributes](#compound-attributes), at every nesting level:
the naming convention is applied to the property name of each nested member, whereas an explicit `PublicName` is used verbatim (it is not converted).
Names of nested members are determined once per compound type, so they are the same wherever the type is used.
Names must be unique within their containing type, so within a resource type or within a compound type.

## Capabilities

_since v4.0_

Default JSON:API attribute capabilities are specified in @JsonApiDotNetCore.Configuration.JsonApiOptions#JsonApiDotNetCore_Configuration_JsonApiOptions_DefaultAttrCapabilities:

```c#
options.DefaultAttrCapabilities = AttrCapabilities.None; // default: All
```

This can be overridden per attribute.

### AllowView

Indicates whether the attribute value can be returned in responses. When not allowed and requested using `?fields[]=`, it results in an HTTP 400 response.
Otherwise, the attribute is silently omitted.

```c#
#nullable enable

public class User : Identifiable<long>
{
    [Attr(Capabilities = ~AttrCapabilities.AllowView)]
    public string Password { get; set; } = null!;
}
```

### AllowFilter

Indicates whether the attribute can be filtered on. When not allowed and used in `?filter=`, an HTTP 400 is returned.

```c#
#nullable enable

public class Person : Identifiable<long>
{
    [Attr(Capabilities = AttrCapabilities.AllowFilter)]
    public string? FirstName { get; set; }
}
```

### AllowSort

Indicates whether the attribute can be sorted on. When not allowed and used in `?sort=`, an HTTP 400 is returned.

```c#
#nullable enable

public class Person : Identifiable<long>
{
    [Attr(Capabilities = ~AttrCapabilities.AllowSort)]
    public string? FirstName { get; set; }
}
```

### AllowCreate

Indicates whether POST requests can assign the attribute value. When sent but not allowed, an HTTP 422 response is returned.

```c#
#nullable enable

public class Person : Identifiable<long>
{
    [Attr(Capabilities = AttrCapabilities.AllowCreate)]
    public string? CreatorName { get; set; }
}
```

### AllowChange

Indicates whether PATCH requests can update the attribute value. When sent but not allowed, an HTTP 422 response is returned.

```c#
#nullable enable

public class Person : Identifiable<long>
{
    [Attr(Capabilities = AttrCapabilities.AllowChange)]
    public string? FirstName { get; set; };
}
```

## Compound attributes

_since v6.0_

A compound attribute holds a structured value: an object with its own named members. Use it for data that belongs to a single resource and
does not need an identity or relationships of its own, such as a name, an address or a point on a map.
To distinguish them from the Entity Framework Core concept of "complex types", we call these types *compound* types.

Compound attributes are first-class: their members are declared using `[Attr]`, just like on resource types,
so names, capabilities, validation, filtering, sorting, sparse fieldsets and change detection work the same way inside them.

> [!NOTE]
> Before v6.0, an `[Attr]` of a class type was serialized using System.Text.Json, which took `[JsonPropertyName]` and `[JsonIgnore]` into account.
> That approach is no longer supported, and neither is persisting such a value as a JSON string next to a `[NotMapped]` property.
> Instead, set `IsCompound = true` on the property and put `[Attr]` on the members of the type.

```c#
#nullable enable

public class Contact : Identifiable<long>
{
    [Attr]
    public string DisplayName { get; set; } = null!;

    [Attr(IsCompound = true)]
    public ContactName Name { get; set; } = null!;

    [Attr(IsCompound = true)]
    public Address? HomeAddress { get; set; }

    [Attr(IsCompound = true)]
    public List<PhoneNumber> PhoneNumbers { get; set; } = [];

    [Attr]
    public List<string> Tags { get; set; } = [];
}

public class ContactName
{
    [Attr]
    public string? FirstName { get; set; }

    [Attr]
    public string LastName { get; set; } = null!;
}

public class Address
{
    [Attr]
    public string Street { get; set; } = null!;

    [Attr(PublicName = "zipCode")]
    public string? PostalCode { get; set; }
}

public class PhoneNumber
{
    [Attr]
    public string Number { get; set; } = null!;

    [Attr]
    public string? Label { get; set; }
}
```

```json
{
  "data": {
    "type": "contacts",
    "id": "1",
    "attributes": {
      "displayName": "Ada",
      "name": { "firstName": "Ada", "lastName": "Lovelace" },
      "homeAddress": null,
      "phoneNumbers": [
        { "number": "555-0100", "label": "mobile" }
      ],
      "tags": [ "friend", "family" ]
    }
  }
}
```

### Mapping with Entity Framework Core

How compound values are stored is up to your ORM. With Entity Framework Core 10 or higher, map them as JSON using
`ComplexProperty(...).ToJson()` and `ComplexCollection(...).ToJson()`:

```c#
protected override void OnModelCreating(ModelBuilder builder)
{
    builder.Entity<Contact>(contact =>
    {
        contact.ComplexProperty(c => c.Name, name => name.ToJson());
        contact.ComplexProperty(c => c.HomeAddress, address => address.ToJson());
        contact.ComplexCollection(c => c.PhoneNumbers, phoneNumber => phoneNumber.ToJson());
    });
}
```

Scalar collections such as `Tags` are mapped as primitive collections by Entity Framework Core.
What can be translated to SQL (for example in filters) depends on the JSON support of your database provider.
The integration tests of JsonApiDotNetCore run against PostgreSQL.

Compound types are not resources: they have no `id`, are not added to the resource graph as resource types, and cannot be reached through a URL of their own.
See [the resource graph](~/usage/resource-graph.md#compound-types).

### Rules for compound types

A compound type is a class or struct. The property can be nullable (`Address?`, or a nullable struct such as `GeoPoint?`).
Its members are declared with `[Attr]`, which supports the same options as on resource types: `PublicName` and `Capabilities` (see [above](#capabilities)).
A getter-only member (for example a calculated value) is read-only. The default capabilities from `options.DefaultAttrCapabilities` apply to nested members too.
Members of a compound type can be compound attributes or collections themselves, to any depth.

```c#
public class Address
{
    [Attr]
    public string Street { get; set; } = null!;

    [Attr(PublicName = "zipCode")]
    public string? PostalCode { get; set; }

    [Attr(Capabilities = ~AttrCapabilities.AllowView)]
    public string? InternalCode { get; set; }

    [Attr]
    public string DisplayText => $"{Street} ({PostalCode})";
}
```

The following is not allowed inside a compound type: relationships (`[HasOne]`/`[HasMany]`), `[EagerLoad]`, and annotating the type with `[Resource]`.
The type must be instantiable: an abstract class or interface is not accepted, and a class needs a parameterless constructor (which may be non-public).
Compound types may not reference themselves, directly or indirectly. These rules are validated at startup, see [the resource graph](~/usage/resource-graph.md#compound-types).

A compound type is shared: when the same CLR type is used by multiple attributes or resource types, it has a single set of members, names and capabilities.
How names of nested members are determined is described [above](#name).
`[JsonPropertyName]`, `[JsonIgnore]` and other System.Text.Json attributes are never taken into account.

### Capabilities of nested members

Capabilities are checked for each member that is involved in a request:

- `AllowView`: A member that cannot be viewed is never returned.
- `AllowFilter` and `AllowSort`: These are checked on every field of the chain. For example, `filter=equals(homeAddress.street,'x')` requires `AllowFilter` on both `homeAddress` and `street`.
  Sorting on a compound or collection attribute itself (instead of on a nested member, or on its count) is not possible.
- `AllowCreate` and `AllowChange`: When a member is sent that does not allow it, an HTTP 422 response is returned, with a source pointer to the member.

```json
{
  "errors": [
    {
      "status": "422",
      "title": "Failed to deserialize request body: Attribute value cannot be assigned when updating resource.",
      "detail": "The attribute 'maidenName' on compound type 'ContactName' cannot be assigned to.",
      "source": {
        "pointer": "/data/attributes/name/maidenName"
      }
    }
  ]
}
```

### Members without `[Attr]`

Public properties of a compound type that do not have `[Attr]` are allowed. They are invisible to the API: they are never read from request bodies,
never written to responses, and cannot be used in query strings. They are still part of the stored value, so your own code (for example a resource definition,
or an overridden service or repository method) can use them to keep, say, a last-modified timestamp or a version number inside the compound value.
JsonApiDotNetCore fetches them before an update and saves them along with the rest of the compound value, and a change to them made on the server counts as a change
when deciding between HTTP 200 and 204 (see [below](#reading-and-writing-compound-values)).

> [!NOTE]
> Because a PATCH replaces the whole compound value, such members are reset to their default value unless your code carries them over from the stored value.

### Collection attributes

_since v6.0_

An attribute whose type is a collection of values is a collection attribute. There are two kinds:

- A *scalar collection*: a `List<T>`, array, set or other collection of simple values (strings, numbers, `Guid`, enums, `DateTime`, `TimeSpan`, ...),
  such as `Tags` in the example above.
- A *compound collection*: a collection of compound types, declared with `[Attr(IsCompound = true)]`, such as `PhoneNumbers` above.

Elements of a collection are not individually addressable: collections are always sent and replaced as a whole.
Collections of collections are not supported. A dictionary is not a collection attribute.
A `byte[]` is a collection of bytes, see [BLOBs](~/usage/advanced/blobs.md).

Whether collection elements may be `null` follows the element type: `null` is accepted for nullable value types (such as `int?`) and for reference types,
and rejected with an HTTP 422 response for non-nullable value types.
See also [nullability](~/usage/resources/nullability.md#compound-and-collection-attributes).

### Reading and writing compound values

Reading: nested members are returned under their public names.
Use [sparse fieldsets](~/usage/reading/sparse-fieldset-selection.md) to select a nested member or an entire compound attribute.
Filtering, sorting and counting on nested members and collections are described in [filtering](~/usage/reading/filtering.md) and [sorting](~/usage/reading/sorting.md).

Writing: a request body assigns the value of an attribute as a whole.

- On POST, members that are omitted keep the value they get from the constructor of the compound type.
- On PATCH, a compound or collection attribute that occurs in the request body *replaces* the stored value entirely. There are no partial updates:
  nested members that are omitted from the request are reset, they do not retain their stored values.
  Attributes that are not in the request body are left untouched. Send the complete value, or leave the attribute out.
- `null` can be assigned to a nullable compound attribute, which clears it. An empty array clears a collection.
- A JSON value of the wrong kind (for example an array where an object is expected) results in HTTP 422, with a source pointer.
- A member that does not exist is handled just like an unknown top-level attribute: ignored when `AllowUnknownFieldsInRequestBody` is `true`, otherwise HTTP 422.
  This applies at every nesting level, including elements of collections.

```http
PATCH /contacts/1 HTTP/1.1
Content-Type: application/vnd.api+json

{
  "data": {
    "type": "contacts",
    "id": "1",
    "attributes": {
      "phoneNumbers": [
        { "number": "555-0199" }
      ]
    }
  }
}
```

The above replaces all phone numbers by a single one without a label. The name and all other attributes remain unchanged.

Error responses use [RFC 6901](https://datatracker.ietf.org/doc/html/rfc6901) JSON Pointers in `source.pointer`. Array elements are addressed by their zero-based index,
and `~` and `/` inside names are escaped as `~0` and `~1`:

```json
{
  "errors": [
    {
      "status": "422",
      "title": "Failed to deserialize request body: Unknown attribute found.",
      "detail": "Attribute 'nope' does not exist on compound type 'PhoneNumber'.",
      "source": {
        "pointer": "/data/attributes/phoneNumbers/0/nope"
      }
    }
  ]
}
```

[Model validation](~/usage/advanced/model-state.md#compound-and-collection-attributes) runs on nested members and collection elements,
with pointers such as `/data/attributes/phoneNumbers/1/number`.

After a PATCH, HTTP 204 is returned when the stored result matches what the request contained and nothing else changed.
Otherwise, HTTP 200 is returned with the resource in the body. Compound and collection values take part in this comparison,
including their members that are not exposed as attributes.
