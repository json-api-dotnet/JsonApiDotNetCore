# Sparse Fieldset Selection

As an alternative to returning all fields (attributes and relationships) from a resource, the `fields[]` query string parameter can be used to select a subset.
Put the resource type to apply the fieldset on between the brackets.
This can be used on primary and secondary endpoints. The selection is applied on both primary and included resources.

Primary endpoint example:

```http
GET /articles?fields[articles]=title,body,comments HTTP/1.1
```

Secondary endpoint example:

```http
GET /api/blogs/1/articles?fields[articles]=title,body,comments HTTP/1.1
```

When combined with the `include` query string parameter, a subset of related fields can be specified too.

Example for an included HasOne relationship:

```http
GET /articles?include=author&fields[authors]=name HTTP/1.1
```

Example for an included HasMany relationship:

```http
GET /articles?include=revisions&fields[revisions]=publishTime HTTP/1.1
```

Example for both top-level and relationship:

```http
GET /articles?include=author&fields[articles]=title,body,author&fields[authors]=name HTTP/1.1
```

> [!NOTE]
> In the last example, the `author` relationship is also added to the `articles` fieldset, so that the relationship from article to author is returned.
When omitted, you'll get the included resources returned, but without full resource linkage (as described [here](https://jsonapi.org/examples/#sparse-fieldsets)).

## Overriding

As a developer, you can force to include and/or exclude specific fields as described [here](~/usage/extensibility/resource-definitions.md).

## Compound attributes and collections

_since v6.0_

Use a dot-separated chain to select a member of a compound attribute, at any nesting depth:

```http
GET /contacts?fields[contacts]=name.lastName,phoneNumbers.number HTTP/1.1
```

Only the selected members are returned (and loaded from the data store). Selecting multiple members of the same compound, or a combination
of nested members and top-level fields, is allowed.

To select a compound attribute entirely, just name it. This returns all of its members that are viewable:

```http
GET /contacts?fields[contacts]=name HTTP/1.1
```

The same applies to collection attributes. Selecting a compound collection (`phoneNumbers`) returns the entire elements, while a chain such as
`phoneNumbers.number` returns only that member for every element. A scalar collection (such as `tags`) is selected by its name.
Compounds nested in compound collections and collections nested in compounds can be selected using longer chains.

When the compound attribute in the database is `null`, it is returned as `null` (when selected as a whole) or omitted from the
response (when only a nested member is selected).

Members that are not viewable (`AllowView` not set on the attribute) are never returned, also not when the compound is selected as a whole.
Selecting such a member explicitly using a chain is rejected with a 400 error, as is selecting a member that does not exist.

Nested chains can be combined with relationships and includes, for example `fields[contacts]=name.lastName,employer&include=employer&fields[companies]=address.city`.
Per-type fieldsets apply the same way to included resources, including those of compound collections nested in included resources.

> [!NOTE]
> Selecting an empty fieldset (`fields[contacts]=`) returns resources with just their type and ID.
