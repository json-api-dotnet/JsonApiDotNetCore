# Caching with ETags

_since v4.2_

GET requests return an [ETag](https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/ETag) HTTP header, which can be used by the client in subsequent requests to save network bandwidth.

Be aware that the returned ETag represents the entire response body (a 'resource' in HTTP terminology) for a request URL that includes the query string.
This is unrelated to JSON:API resources. Therefore, we do not use ETags for optimistic concurrency.

Getting a list of resources returns an ETag:

```http
GET /articles?sort=-lastModifiedAt HTTP/1.1
Host: localhost:5000
```

```http
HTTP/1.1 200 OK
Content-Type: application/vnd.api+json
Server: Kestrel
Transfer-Encoding: chunked
ETag: "7FFF010786E2CE8FC901896E83870E00"

{
  "data": [ ... ]
}
```

The request is later resent using the received ETag. The server data has not changed at this point.

```http
GET /articles?sort=-lastModifiedAt HTTP/1.1
Host: localhost:5000
If-None-Match: "7FFF010786E2CE8FC901896E83870E00"
```

```http
HTTP/1.1 304 Not Modified
Server: Kestrel
ETag: "7FFF010786E2CE8FC901896E83870E00"
```

After some time, the server data has changed.

```http
GET /articles?sort=-lastModifiedAt HTTP/1.1
Host: localhost:5000
If-None-Match: "7FFF010786E2CE8FC901896E83870E00"
```

```http
HTTP/1.1 200 OK
Content-Type: application/vnd.api+json
Server: Kestrel
Transfer-Encoding: chunked
ETag: "356075D903B8FE8D9921201A7E7CD3F9"

{
  "data": [ ... ]
}
```

**Note:** To just poll for changes (without fetching them), send a HEAD request instead:

```http
HEAD /articles?sort=-lastModifiedAt HTTP/1.1
Host: localhost:5000
If-None-Match: "7FFF010786E2CE8FC901896E83870E00"
```

```http
HTTP/1.1 200 OK
Server: Kestrel
ETag: "356075D903B8FE8D9921201A7E7CD3F9"
```
