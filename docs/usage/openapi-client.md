# OpenAPI clients

After [enabling OpenAPI](~/usage/openapi.md), you can generate a typed JSON:API client for your API in various programming languages.

> [!NOTE]
> If you prefer a generic JSON:API client instead of a typed one, choose from the existing
> [client libraries](https://jsonapi.org/implementations/#client-libraries).

The following code generators are supported, though you may try others as well:
- [NSwag](https://github.com/RicoSuter/NSwag): Produces clients for C# and TypeScript
- [Kiota](https://learn.microsoft.com/en-us/openapi/kiota/overview): Produces clients for C#, Go, Java, PHP, Python, Ruby, Swift and TypeScript

# [NSwag](#tab/nswag)

For C# clients, we provide an additional package that provides workarounds for bugs in NSwag and enables using partial PATCH/POST requests.

To add it to your project, run the following command:
```
dotnet add package JsonApiDotNetCore.OpenApi.Client.NSwag
```

# [Kiota](#tab/kiota)

For C# clients, we provide an additional package that provides workarounds for bugs in Kiota.

To add it to your project, run the following command:
```
dotnet add package JsonApiDotNetCore.OpenApi.Client.Kiota
```

---

## Getting started

To generate your C# client, follow the steps below.

# [NSwag](#tab/nswag)

### Visual Studio

The easiest way to get started is by using the built-in capabilities of Visual Studio.
The following steps describe how to generate and use a JSON:API client in C#, using our package.

1.  In **Solution Explorer**, right-click your client project, select **Add** > **Service Reference** and choose **OpenAPI**.

2.  On the next page, specify the OpenAPI URL to your JSON:API server, for example: `http://localhost:14140/swagger/v1/swagger.json`.
    Specify `ExampleApiClient` as the class name, optionally provide a namespace and click **Finish**.
    Visual Studio now downloads your swagger.json and updates your project file.
    This adds a pre-build step that generates the client code.

    > [!TIP]
    > To later re-download swagger.json and regenerate the client code,
    > right-click **Dependencies** > **Manage Connected Services** and click the **Refresh** icon.

3.  Although not strictly required, we recommend running package update now, which fixes some issues.

    > [!WARNING]
    > NSwag v14 is currently *incompatible* with JsonApiDotNetCore (tracked [here](https://github.com/RicoSuter/NSwag/issues/4662)).
    > Stick with v13.x for the moment.

4.  Add our client package to your project:

    ```
    dotnet add package JsonApiDotNetCore.OpenApi.Client.NSwag
    ```

5.  Add the following line inside the **OpenApiReference** section in your project file:

    ```xml
    <Options>/GenerateExceptionClasses:false /GenerateNullableReferenceTypes:true /GenerateOptionalPropertiesAsNullable:true /GenerateOptionalParameters:true /AdditionalNamespaceUsages:JsonApiDotNetCore.OpenApi.Client.NSwag</Options>
    ```

6.  Add the following glue code to connect our package with your generated code.

    > [!NOTE]
    > The class name must be the same as specified in step 2.
    > If you also specified a namespace, put this class in the same namespace.
    > For example, add `namespace GeneratedCode;` below the `using` lines.

    ```c#
    using JsonApiDotNetCore.OpenApi.Client.NSwag;
    using Newtonsoft.Json;

    partial class ExampleApiClient : JsonApiClient
    {
        partial void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
        {
            SetSerializerSettingsForJsonApi(settings);
        }
    }
    ```

7.  Add code that calls one of your JSON:API endpoints.

    ```c#
    using var httpClient = new HttpClient();
    var apiClient = new ExampleApiClient(httpClient);

    var getResponse = await apiClient.GetPersonCollectionAsync(new Dictionary<string, string?>
    {
        ["filter"] = "has(assignedTodoItems)",
        ["sort"] = "-lastName",
        ["page[size]"] = "5"
    }, null);

    foreach (var person in getResponse.Data)
    {
        Console.WriteLine($"Found person {person.Id}: {person.Attributes!.DisplayName}");
    }
    ```

8.  Extend your demo code to send a partial PATCH request with the help of our package:

    ```c#
    var updatePersonRequest = new UpdatePersonRequestDocument
    {
        Data = new DataInUpdatePersonRequest
        {
            Id = "1",
            Attributes = new AttributesInUpdatePersonRequest
            {
                LastName = "Doe"
            }
        }
    };

    // This line results in sending "firstName: null" instead of omitting it.
    using (apiClient.WithPartialAttributeSerialization<UpdatePersonRequestDocument, AttributesInUpdatePersonRequest>(
        updatePersonRequest, person => person.FirstName))
    {
        // Workaround for https://github.com/RicoSuter/NSwag/issues/2499.
        await ApiResponse.TranslateAsync(() =>
            apiClient.PatchPersonAsync(updatePersonRequest.Data.Id, updatePersonRequest));

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

> [!TIP]
> The [example project](https://github.com/json-api-dotnet/JsonApiDotNetCore/tree/openapi/src/Examples/OpenApiNSwagClientExample) contains an enhanced version
> that uses `IHttpClientFactory` for [scalability](https://learn.microsoft.com/en-us/dotnet/core/extensions/httpclient-factory) and
> [resiliency](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-requests#use-polly-based-handlers) and logs the HTTP requests and responses.
> Additionally, the example shows how to write the swagger.json file to disk when building the server, which is imported from the client project.
> This keeps the server and client automatically in sync, which is handy when both are in the same solution.

### Other IDEs

When using the command line, you can try the [Microsoft.dotnet-openapi Global Tool](https://docs.microsoft.com/en-us/aspnet/core/web-api/microsoft.dotnet-openapi?view=aspnetcore-5.0).

Alternatively, the following section shows what to add to your client project file directly:

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.Extensions.ApiDescription.Client" Version="8.0.*" PrivateAssets="all" />
  <PackageReference Include="Newtonsoft.Json" Version="13.0.*" />
  <PackageReference Include="NSwag.ApiDescription.Client" Version="13.20.*" PrivateAssets="all" />
</ItemGroup>

<ItemGroup>
  <OpenApiReference Include="OpenAPIs\swagger.json">
    <SourceUri>http://localhost:14140/swagger/v1/swagger.json</SourceUri>
      <CodeGenerator>NSwagCSharp</CodeGenerator>
      <ClassName>ExampleApiClient</ClassName>
      <OutputPath>ExampleApiClient.cs</OutputPath>
      <Options>/GenerateExceptionClasses:false /GenerateNullableReferenceTypes:true /GenerateOptionalPropertiesAsNullable:true /GenerateOptionalParameters:true /AdditionalNamespaceUsages:JsonApiDotNetCore.OpenApi.Client.NSwag</Options>
  </OpenApiReference>
</ItemGroup>
```

From here, continue from step 3 in the list of steps for Visual Studio.

# [Kiota](#tab/kiota)

To generate your C# client, install the Kiota tool by following the steps at https://learn.microsoft.com/en-us/openapi/kiota/install#install-as-net-tool.

Next, generate client code by running the [command line tool](https://learn.microsoft.com/en-us/openapi/kiota/using#client-generation). For example:

```
dotnet kiota generate --language CSharp --class-name ExampleApiClient --output ./GeneratedCode --backing-store --exclude-backward-compatible --clean-output --clear-cache --openapi http://localhost:14140/swagger/v1/swagger.json
```

> [!CAUTION]
> The `--backing-store` switch is needed for JSON:API partial PATCH/POST requests to work correctly.

Kiota is pretty young and therefore still rough around the edges. At the time of writing, there are various bugs, for which we have workarounds
in place. For a full example, see the [example project](https://github.com/json-api-dotnet/JsonApiDotNetCore/tree/openapi/src/Examples/OpenApiKiotaClientExample).

---

## Configuration

Various switches enable you to tweak the client generation to your needs. See the section below for an overview.

# [NSwag](#tab/nswag)

The `OpenApiReference` element in the project file accepts an `Options` element to pass additional settings to the client generator,
which are listed [here](https://github.com/RicoSuter/NSwag/blob/master/src/NSwag.Commands/Commands/CodeGeneration/OpenApiToCSharpClientCommand.cs).
A guide with common best practices is available [here](https://stevetalkscode.co.uk/openapireference-commands).

For example, the following section puts the generated code in a namespace and generates an interface (which is handy for dependency injection):

```xml
<OpenApiReference Include="swagger.json">
  <Namespace>ExampleProject.GeneratedCode</Namespace>
  <ClassName>SalesApiClient</ClassName>
  <CodeGenerator>NSwagCSharp</CodeGenerator>
  <Options>/GenerateClientInterfaces:true</Options>
</OpenApiReference>
```

Likewise, you can enable nullable reference types by adding `/GenerateNullableReferenceTypes:true /GenerateOptionalPropertiesAsNullable:true /GenerateOptionalParameters:true`.

# [Kiota](#tab/kiota)

The available command-line switches for Kiota are described [here](https://learn.microsoft.com/en-us/openapi/kiota/using#client-generation).

At the time of writing, Kiota provides [no official integration](https://github.com/microsoft/kiota/issues/3005) with MSBuild.
Our [example project](https://github.com/json-api-dotnet/JsonApiDotNetCore/tree/openapi/src/Examples/OpenApiKiotaClientExample) takes a stab at it,
although it has glitches. If you're an MSBuild expert, please help out!

```xml
<Target Name="KiotaRunTool" BeforeTargets="BeforeCompile;CoreCompile" Condition="$(DesignTimeBuild) != true And $(BuildingProject) == true">
  <Exec
    Command="dotnet kiota generate --language CSharp --class-name ExampleApiClient --namespace-name OpenApiKiotaClientExample.GeneratedCode --output ./GeneratedCode --backing-store --exclude-backward-compatible --clean-output --clear-cache --openapi ..\JsonApiDotNetCoreExample\GeneratedSwagger\JsonApiDotNetCoreExample.json" />

  <ItemGroup>
    <!-- This isn't entirely reliable: may require a second build after the source swagger.json has changed, to get rid of compile errors. -->
    <Compile Remove="**\GeneratedCode\**\*.cs" />
    <Compile Include="**\GeneratedCode\**\*.cs" />
  </ItemGroup>
</Target>
```

---

## Headers and caching

The use of HTTP headers varies per client generator. To use [ETags for caching](~/usage/caching.md), see the notes below.

# [NSwag](#tab/nswag)

NSwag needs extra settings to make response headers accessible. Specify the following in the `<Options>` element of your project file:

```
/GenerateExceptionClasses:false /WrapResponses:true /GenerateResponseClasses:false /ResponseClass:ApiResponse /AdditionalNamespaceUsages:JsonApiDotNetCore.OpenApi.Client.NSwag
```

This enables the following code, which is explained below:

```c#
var getResponse = await ApiResponse.TranslateAsync(() => apiClient.GetPersonCollectionAsync());
string eTag = getResponse.Headers["ETag"].Single();
Console.WriteLine($"Retrieved {getResponse.Result?.Data.Count ?? 0} people.");

// wait some time...

getResponse = await ApiResponse.TranslateAsync(() => apiClient.GetPersonCollectionAsync(null, eTag));

if (getResponse is { StatusCode: (int)HttpStatusCode.NotModified, Result: null })
{
    Console.WriteLine("The HTTP response hasn't changed, so no response body was returned.");
}
```

The response of the first API call contains both data and an ETag header, which is a fingerprint of the response.
That ETag gets passed to the second API call. This enables the server to detect if something changed, which optimizes
network usage: no data is sent back, unless is has changed.
If you only want to ask whether data has changed without fetching it, use a HEAD request instead.

# [Kiota](#tab/kiota)

Use `HeadersInspectionHandlerOption` to gain access to response headers. For example:

```c#
var headerInspector = new HeadersInspectionHandlerOption
{
    InspectResponseHeaders = true
};

var responseDocument = await apiClient.Api.People.GetAsync(configuration => configuration.Options.Add(headerInspector));

string eTag = headerInspector.ResponseHeaders["ETag"].Single();
```

Due to a [bug in Kiota](https://github.com/microsoft/kiota/issues/4190), a try/catch block is needed additionally to make this work.

For a full example, see the [example project](https://github.com/json-api-dotnet/JsonApiDotNetCore/tree/openapi/src/Examples/OpenApiKiotaClientExample).

---

## Atomic operations

# [NSwag](#tab/nswag)

[Atomic operations](~/usage/writing/bulk-batch-operations.md) are fully supported.
The [example project](https://github.com/json-api-dotnet/JsonApiDotNetCore/tree/openapi/src/Examples/OpenApiNSwagClientExample)
demonstrates how to use them. It uses local IDs to:
- Create a new tag
- Create a new person
- Create a new todo-item, tagged with the new tag, and owned by the new person
- Assign the todo-item to the created person

# [Kiota](#tab/kiota)

[Atomic operations](~/usage/writing/bulk-batch-operations.md) are fully supported.
See the [example project](https://github.com/json-api-dotnet/JsonApiDotNetCore/tree/openapi/src/Examples/OpenApiKiotaClientExample)
demonstrates how to use them. It uses local IDs to:
- Create a new tag
- Create a new person
- Create a new todo-item, tagged with the new tag, and owned by the new person
- Assign the todo-item to the created person

---
