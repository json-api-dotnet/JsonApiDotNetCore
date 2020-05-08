# Sparse Field Selection

We currently support top-level and single-depth nested field selection using the fields query parameter.

Top-level example:
```http
GET /articles?fields=title,body HTTP/1.1
```

Example for included relationship:
```http
GET /articles?include=author&fields[author]=name HTTP/1.1
```

Or both:
```http
GET /articles?fields=title,body&include=author&fields[author]=name HTTP/1.1
```
