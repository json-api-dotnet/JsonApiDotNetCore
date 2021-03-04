# Sparse Fieldset Selection

As an alternative to returning all fields (attributes and relationships) from a resource, the `fields[]` query string parameter can be used to select a subset.
Put the resource type to apply the fieldset on between the brackets.
This can be used on the resource being requested, as well as on nested endpoints and/or included resources.

Top-level example:

```http
GET /articles?fields[articles]=title,body,comments HTTP/1.1
```

Nested endpoint example:

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

Note that in the last example, the `author` relationship is also added to the `articles` fieldset, so that the relationship from article to author is returned.
When omitted, you'll get the included resources returned, but without full resource linkage (as described [here](https://jsonapi.org/examples/#sparse-fieldsets)).

## Overriding

As a developer, you can force to include and/or exclude specific fields as [described previously](~/usage/resources/resource-definitions.md).
