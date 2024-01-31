# OpenAPI Client

You can generate a JSON:API client in various programming languages from the [OpenAPI specification](https://swagger.io/specification/) file that JsonApiDotNetCore APIs provide.

For C# .NET clients generated using [NSwag](https://github.com/RicoSuter/NSwag), we provide an additional package
that introduces support for partial PATCH/POST requests. The concern here is that a property on a generated C# class
being `null` could mean "set the value to `null` in the request" or "this is `null` because I never touched it".

## Getting started

### Visual Studio

The easiest way to get started is by using the built-in capabilities of Visual Studio.
The next steps describe how to generate a JSON:API client library and use our package.

1.  In **Solution Explorer**, right-click your client project, select **Add** > **Service Reference** and choose **OpenAPI**.

2.  On the next page, specify the OpenAPI URL to your JSON:API server, for example: `http://localhost:14140/swagger/v1/swagger.json`.
    Specify `ExampleApiClient` as class name, optionally provide a namespace and click **Finish**.
    Visual Studio now downloads your swagger.json and updates your project file.
    This adds a pre-build step that generates the client code.

    > [!TIP]
    > To later re-download swagger.json and regenerate the client code,
    > right-click **Dependencies** > **Manage Connected Services** and click the **Refresh** icon.

3.  Although not strictly required, we recommend to run package update now, which fixes some issues.

4.  Add code that calls one of your JSON:API endpoints.

    ```c#
    using var httpClient = new HttpClient();
    var apiClient = new ExampleApiClient(httpClient);

    PersonCollectionResponseDocument getResponse = await apiClient.GetPersonCollectionAsync(new Dictionary<string, string?>
    {
        ["filter"] = "has(assignedTodoItems)",
        ["sort"] = "-lastName",
        ["page[size]"] = "5"
    });

    foreach (PersonDataInResponse person in getResponse.Data)
    {
        Console.WriteLine($"Found person {person.Id}: {person.Attributes.DisplayName}");
    }
    ```

5.  Add our client package to your project:

    ```
    dotnet add package JsonApiDotNetCore.OpenApi.Client
    ```

6.  Add the following glue code to connect our package with your generated code.

    > [!NOTE]
    > The class name must be the same as specified in step 2.
    > If you also specified a namespace, put this class in the same namespace.
    > For example, add `namespace GeneratedCode;` below the `using` lines.

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

    > [!TIP]
    > The project at src/Examples/JsonApiDotNetCoreExampleClient contains an enhanced version that logs the HTTP requests and responses.

7.  Extend your demo code to send a partial PATCH request with the help of our package:

    ```c#
    var patchRequest = new PersonPatchRequestDocument
    {
        Data = new PersonDataInPatchRequest
        {
            Id = "1",
            Attributes = new PersonAttributesInPatchRequest
            {
                LastName = "Doe"
            }
        }
    };

    // This line results in sending "firstName: null" instead of omitting it.
    using (apiClient.WithPartialAttributeSerialization<PersonPatchRequestDocument, PersonAttributesInPatchRequest>(patchRequest,
        person => person.FirstName))
    {
        // Workaround for https://github.com/RicoSuter/NSwag/issues/2499.
        await TranslateAsync(async () => await apiClient.PatchPersonAsync(patchRequest.Data.Id, null, patchRequest));

        // The sent request looks like this:
        // {
        //   "data": {
        //     "type": "people",
        //     "id": "1",
        //     "attributes": {
        //       "firstName": null,
        //       "lastName": "Doe"
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
  <PackageReference Include="Microsoft.Extensions.ApiDescription.Client" Version="7.0.11">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
  </PackageReference>
  <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
  <PackageReference Include="NSwag.ApiDescription.Client" Version="13.20.0">
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

For example, the next section puts the generated code in a namespace and generates an interface (which is handy for dependency injection):

```xml
<OpenApiReference Include="swagger.json">
  <Namespace>ExampleProject.GeneratedCode</Namespace>
  <ClassName>SalesApiClient</ClassName>
  <CodeGenerator>NSwagCSharp</CodeGenerator>
  <Options>/GenerateClientInterfaces:true</Options>
</OpenApiReference>
```
