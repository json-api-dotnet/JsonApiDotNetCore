# OpenAPI Client

You can generate a JSON:API client in various programming languages from the [OpenAPI specification](https://swagger.io/specification/) file that JsonApiDotNetCore APIs provide.

For C# .NET clients generated using [NSwag](https://github.com/RicoSuter/NSwag), we provide an additional package that introduces support for partial PATCH/POST requests. The issue here is that a property on a generated C# class being `null` could mean "set the value to `null` in the request" or "this is `null` because I never touched it".

## Getting started

### Visual Studio

The easiest way to get started is by using the built-in capabilities of Visual Studio. The next steps describe how to generate a JSON:API client library and use our package.

1.  In **Solution Explorer**, right-click your client project, select **Add** > **Service Reference** and choose **OpenAPI**.

2.  On the next page, specify the OpenAPI URL to your JSON:API server, for example: `http://localhost:14140/swagger/v1/swagger.json`.
    Optionally provide a class name and namespace and click **Finish**.
    Visual Studio now downloads your swagger.json and updates your project file. This results in a pre-build step that generates the client code.

    Tip: To later re-download swagger.json and regenerate the client code, right-click **Dependencies** > **Manage Connected Services** and click the **Refresh** icon.
3.  Although not strictly required, we recommend to run package update now, which fixes some issues and removes the `Stream` parameter from generated calls.

4.  Add some demo code that calls one of your JSON:API endpoints. For example:

    ```c#
    using var httpClient = new HttpClient();
    var apiClient = new ExampleApiClient("http://localhost:14140", httpClient);

    PersonCollectionResponseDocument getResponse =
        await apiClient.GetPersonCollectionAsync();

    foreach (PersonDataInResponse person in getResponse.Data)
    {
        Console.WriteLine($"Found user {person.Id} named " +
            $"'{person.Attributes.FirstName} {person.Attributes.LastName}'.");
    }
    ```

5.  Add our client package to your project:

   ```
   dotnet add package JsonApiDotNetCore.OpenApi.Client
   ```

6.  Add the following glue code to connect our package with your generated code. The code below assumes you specified `ExampleApiClient` as class name in step 2.

    ```c#
    using JsonApiDotNetCore.OpenApi.Client;
    using Newtonsoft.Json;

    partial class ExampleApiClient : JsonApiClient
    {
        partial void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
        {
            SetSerializerSettingsForJsonApi(settings);
        }
    }
    ```

7.  Extend your demo code to send a partial PATCH request with the help of our package:

    ```c#
    var patchRequest = new PersonPatchRequestDocument
    {
        Data = new PersonDataInPatchRequest
        {
            Id = "1",
            Attributes = new PersonAttributesInPatchRequest
            {
                FirstName = "Jack"
            }
        }
    };

    // This line results in sending "lastName: null" instead of omitting it.
    using (apiClient.RegisterAttributesForRequestDocument<PersonPatchRequestDocument,
        PersonAttributesInPatchRequest>(patchRequest, person => person.LastName))
    {
        PersonPrimaryResponseDocument patchResponse =
            await apiClient.PatchPersonAsync("1", patchRequest);

        // The sent request looks like this:
        // {
        //   "data": {
        //     "type": "people",
        //     "id": "1",
        //     "attributes": {
        //       "firstName": "Jack",
        //       "lastName": null
        //     }
        //   }
        // }
    }
    ```

### Other IDEs

When using the command-line, you can try the [Microsoft.dotnet-openapi Global Tool](https://docs.microsoft.com/en-us/aspnet/core/web-api/microsoft.dotnet-openapi?view=aspnetcore-5.0).

Alternatively, the next section shows what to add to your client project file directly:

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.Extensions.ApiDescription.Client" Version="3.0.0">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
  </PackageReference>
  <PackageReference Include="Newtonsoft.Json" Version="12.0.2" />
  <PackageReference Include="NSwag.ApiDescription.Client" Version="13.0.5">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
  </PackageReference>
</ItemGroup>

<ItemGroup>
  <OpenApiReference Include="OpenAPIs\swagger.json" CodeGenerator="NSwagCSharp" ClassName="ExampleApiClient">
    <SourceUri>http://localhost:14140/swagger/v1/swagger.json</SourceUri>
  </OpenApiReference>
</ItemGroup>
```

From here, continue from step 3 in the list of steps for Visual Studio.

## Configuration

### NSwag

The `OpenApiReference` element in the project file accepts an `Options` element to pass additional settings to the client generator,
which are listed [here](https://github.com/RicoSuter/NSwag/blob/master/src/NSwag.Commands/Commands/CodeGeneration/OpenApiToCSharpClientCommand.cs).

For example, the next section puts the generated code in a namespace, removes the `baseUrl` parameter and generates an interface (which is handy for dependency injection):

```xml
<OpenApiReference Include="swagger.json">
  <Namespace>ExampleProject.GeneratedCode</Namespace>
  <ClassName>SalesApiClient</ClassName>
  <CodeGenerator>NSwagCSharp</CodeGenerator>
  <Options>/UseBaseUrl:false /GenerateClientInterfaces:true</Options>
</OpenApiReference>
```
