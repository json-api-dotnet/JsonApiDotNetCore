# Request Examples

To update these requests:

1. Add a PowerShell (.ps1) script prefixed by a number that is used to determine the order the scripts are executed. The script should execute a request and output the response. Example:
```
curl -s http://localhost:14141/api/books
```

2. Add the example to `index.md`. Example:
```
### Get with relationship

[!code-ps[REQUEST](003_GET_Books-including-Author.ps1)]
[!code-json[RESPONSE](003_GET_Books-including-Author_Response.json)]
```

3. Run `./generate-examples.ps1`
4. Verify the results by running `docfx --serve`
