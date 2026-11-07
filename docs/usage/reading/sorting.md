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

## Nested attributes

_since v6.0_

The path to sort on can go through to-one relationships and compound attributes, in any combination.

```http
GET /api/contacts?sort=name.lastName,-name.firstName HTTP/1.1
```

```http
GET /api/contacts?sort=employer.address.city HTTP/1.1
```

The last field in the path must be a scalar attribute. Sorting on a compound attribute, on a collection or on a relationship itself is not possible.

## Count

To sort on the number of nested resources, use the `count` function.

```http
GET /api/blogs?sort=count(articles) HTTP/1.1
```

This sorts the list of blogs by their number of articles.

_since v6.0_

The `count` function also works on collection attributes: scalar collections, compound collections, and those nested in compound attributes
or reached through to-one relationships.

```http
GET /api/contacts?sort=-count(phoneNumbers),count(tags) HTTP/1.1
```

```http
GET /api/contacts?sort=count(name.nicknames) HTTP/1.1
```

A collection that is `null` counts as empty.

## Restricting sorting

By default, all attributes can be sorted on. Use `AttrCapabilities.AllowSort` to opt out, which works for attributes
of compound types too:

```c#
#nullable enable

public sealed class Contact : Identifiable<long>
{
    [Attr(Capabilities = AttrCapabilities.AllowView | AttrCapabilities.AllowFilter)]
    public string? Notes { get; set; }
}
```

The capability is checked on every attribute in the path, including intermediate compound attributes and the collection in `count(...)`.
For example, when the `Name` compound attribute does not have `AllowSort`, then neither `sort=name.lastName` nor `sort=count(name.nicknames)` is allowed.

## Secondary endpoints

Sorting can be used on secondary endpoints, such as:

```http
GET /api/blogs/1/articles?sort=caption HTTP/1.1
```

and on included resources, for example:

```http
GET /api/blogs/1/articles?include=revisions&sort=caption&sort[revisions]=publishTime HTTP/1.1
```

This sorts the list of blogs by their captions and included revisions by their publication time.

## Default Sort

See the topic on [Resource Definitions](~/usage/extensibility/resource-definitions.md)
for overriding the default sort behavior.
