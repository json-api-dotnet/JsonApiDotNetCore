# Request Examples

To update these requests:

1. Add a PowerShell (`.ps1`) script prefixed by a number that is used to determine the order the scripts are executed.
  The script should execute a request and output the response. For example:
   ```
   curl -s http://localhost:14141/api/books
   ```

2. Add the example to `index.md`. For example:
   ```
   ### Get with relationship

   [!code-ps[REQUEST](003_GET_Books-including-Author.ps1)]
   [!code-json[RESPONSE](003_GET_Books-including-Author_Response.json)]
   ```

3. Run `pwsh ../generate-examples.ps1` to execute the request.

4. Run `pwsh ../build-dev.ps1` to view the output on the website.
