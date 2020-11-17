# Deleting resources

A single resource can be deleted using a DELETE request. The next example deletes an article:

```http
DELETE /articles/1 HTTP/1.1
```

This returns `204 No Content` if the resource was successfully deleted. Alternatively, if the resource does not exist, `404 Not Found` is returned.
