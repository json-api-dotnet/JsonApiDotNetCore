# Request Examples

To update these requests:

1. Add a bash (.sh) file prefixed by a number that is used to determine the order the scripts are executed. The bash script should execute a request and output the response. Example:
```
curl -vs http://localhost:5001/api/articles
```

2. Add the example to `index.md`. Example:
```
## Get Article with Author

[!code-sh[GET Request](004-GET_Articles_With_Authors.sh)]
[!code-json[GET Response](004-GET_Articles_With_Authors-Response.json)]
```

3. Run `./generate.sh`
4. Verify the results by running `docfx --serve`
