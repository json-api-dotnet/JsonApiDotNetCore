# Sparse Field Selection

We currently support top-level field selection. What this means is you can restrict which fields are returned by a query using the fields query parameter, but this does not yet apply to included relationships.

```http
GET /articles?fields[articles]=title,body HTTP/1.1
Accept: application/vnd.api+json
```