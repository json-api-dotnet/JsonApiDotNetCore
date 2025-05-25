> [!WARNING]
> OpenAPI support for JSON:API is currently experimental. The API and the structure of the OpenAPI document may change in future versions.

# OpenAPI clients

After [enabling OpenAPI](~/usage/openapi.md), you can generate a typed JSON:API client for your API in various programming languages.

> [!NOTE]
> If you prefer a generic JSON:API client instead of a typed one, choose from the existing
> [client libraries](https://jsonapi.org/implementations/#client-libraries).

The following code generators are supported, though you may try others as well:
- [NSwag](https://github.com/RicoSuter/NSwag) (v14.1 or higher): Produces clients for C# (requires `Newtonsoft.Json`) and TypeScript
- [Kiota](https://learn.microsoft.com/openapi/kiota/overview): Produces clients for C#, Go, Java, PHP, Python, Ruby, Swift and TypeScript

# [NSwag](#tab/nswag)

For C# clients, we provide an additional package that provides workarounds for bugs in NSwag and enables using partial POST/PATCH requests.

To add it to your project, run the following command:
```
dotnet add package JsonApiDotNetCore.OpenApi.Client.NSwag --prerelease
```

# [Kiota](#tab/kiota)

For C# clients, we provide an additional package that provides workarounds for bugs in Kiota, as well as MSBuild integration.

To add it to your project, run the following command:
```
dotnet add package JsonApiDotNetCore.OpenApi.Client.Kiota --prerelease
```

---

## Getting started

To generate your C# client, follow the steps below.

# [NSwag](#tab/nswag)

### Visual Studio

The easiest way to get started is by using the built-in capabilities of Visual Studio.
The following steps describe how to generate and use a JSON:API client in C#, combined with our NuGet package.

1.  In **Solution Explorer**, right-click your client project, select **Add** > **Service Reference** and choose **OpenAPI**.

1.  On the next page, specify the OpenAPI URL to your JSON:API server, for example: `http://localhost:14140/swagger/v1/swagger.json`.
    Specify `ExampleApiClient` as the class name, optionally provide a namespace and click **Finish**.
    Visual Studio now downloads your swagger.json and updates your project file.
    This adds a pre-build step that generates the client code.

    > [!TIP]
    > To later re-download swagger.json and regenerate the client code,
    > right-click **Dependencies** > **Manage Connected Services** and click the **Refresh** icon.

1.  Run package update now, which fixes incompatibilities and bugs from older versions.

1.  Add our client package to your project:

    ```
    dotnet add package JsonApiDotNetCore.OpenApi.Client.NSwag --prerelease
    ```

1.  Add code that calls one of your JSON:API endpoints.

    ```c#
    using var httpClient = new HttpClient();
    var apiClient = new ExampleApiClient(httpClient);

    var getResponse = await apiClient.GetPersonCollectionAsync(new Dictionary<string, string?>
    {
        ["filter"] = "has(assignedTodoItems)",
        ["sort"] = "-lastName",
        ["page[size]"] = "5"
    });

    foreach (var person in getResponse.Data)
    {
        Console.WriteLine($"Found person {person.Id}: {person.Attributes!.DisplayName}");
    }
    ```

1.  Extend the demo code to send a partial PATCH request with the help of our package:

    ```c#
    var updatePersonRequest = new UpdatePersonRequestDocument
    {
        Data = new DataInUpdatePersonRequest
        {
            Id = "1",
            // Using TrackChangesFor to send "firstName: null" instead of omitting it.
            Attributes = new TrackChangesFor<AttributesInUpdatePersonRequest>(_apiClient)
            {
                Initializer =
                {
                    FirstName = null,
                    LastName = "Doe"
                }
            }.Initializer
        }
    };

    await ApiResponse.TranslateAsync(async () =>
        await _apiClient.PatchPersonAsync(updatePersonRequest.Data.Id, updatePersonRequest));

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
    ```

> [!TIP]
> The [example project](https://github.com/json-api-dotnet/JsonApiDotNetCore/tree/master/src/Examples/OpenApiNSwagClientExample) contains an enhanced version
> that uses `IHttpClientFactory` for [scalability](https://learn.microsoft.com/dotnet/core/extensions/httpclient-factory) and
> [resiliency](https://learn.microsoft.com/aspnet/core/fundamentals/http-requests#use-polly-based-handlers) and logs the HTTP requests and responses.
> Additionally, the example shows how to write the swagger.json file to disk when building the server, which is imported from the client project.
> This keeps the server and client automatically in sync, which is handy when both are in the same solution.

### Other IDEs

The following section shows what to add to your client project file directly:

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.Extensions.ApiDescription.Client" Version="8.0.*" PrivateAssets="all" />
  <PackageReference Include="Newtonsoft.Json" Version="13.0.*" />
  <PackageReference Include="NSwag.ApiDescription.Client" Version="14.1.*" PrivateAssets="all" />
</ItemGroup>

<ItemGroup>
  <OpenApiReference Include="OpenAPIs\swagger.json">
    <SourceUri>http://localhost:14140/swagger/v1/swagger.json</SourceUri>
    <ClassName>ExampleApiClient</ClassName>
    <OutputPath>%(ClassName).cs</OutputPath>
  </OpenApiReference>
</ItemGroup>
```

From here, continue from step 3 in the list of steps for Visual Studio.

# [Kiota](#tab/kiota)

To generate your C# client, first add the Kiota tool to your solution:

```
dotnet tool install microsoft.openapi.kiota
```

After adding the `JsonApiDotNetCore.OpenApi.Client.Kiota` package to your project, add a `KiotaReference` element
to your project file to import your OpenAPI file. For example:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <KiotaReference Include="path/to/openapi.json">
      <NamespaceName>$(MSBuildProjectName).GeneratedCode</NamespaceName>
      <ClassName>ExampleApiClient</ClassName>
      <OutputPath>./GeneratedCode</OutputPath>
      <ExtraArguments>$(JsonApiExtraArguments)</ExtraArguments>
    </KiotaReference>
  </ItemGroup>
</Project>
```

> [!NOTE]
> The `ExtraArguments` parameter is required for compatibility with JSON:API.

Next, build your project. It runs the kiota command-line tool, which generates files in the `GeneratedCode` subdirectory.

> [!CAUTION]
> If you're not using `<KiotaReference>`, at least make sure you're passing the `--backing-store` switch to the command-line tool,
> which is needed for JSON:API partial POST/PATCH requests to work correctly.

Kiota is pretty young and therefore still rough around the edges. At the time of writing, there are various bugs, for which we have workarounds
in place. For a full example, see the [example project](https://github.com/json-api-dotnet/JsonApiDotNetCore/tree/master/src/Examples/OpenApiKiotaClientExample).

---

## Configuration

Various switches enable you to tweak the client generation to your needs. See the section below for an overview.

# [NSwag](#tab/nswag)

The `OpenApiReference` element can be customized using various [NSwag-specific MSBuild properties](https://github.com/RicoSuter/NSwag/blob/7d6df3af95081f3f0ed6dee04be8d27faa86f91a/src/NSwag.ApiDescription.Client/NSwag.ApiDescription.Client.props).
See [the source code](https://github.com/RicoSuter/NSwag/blob/master/src/NSwag.Commands/Commands/CodeGeneration/OpenApiToCSharpClientCommand.cs) for their meaning.
The `JsonApiDotNetCore.OpenApi.Client.NSwag` package sets various of these for optimal JSON:API support.

> [!NOTE]
> Earlier versions of NSwag required the use of `<Options>` to specify command-line switches directly.
> This is no longer recommended and may conflict with the new MSBuild properties.

For example, the following section puts the generated code in a namespace, makes the client class internal and generates an interface (handy when writing tests):

```xml
<OpenApiReference Include="swagger.json">
  <Namespace>ExampleProject.GeneratedCode</Namespace>
  <NSwagClientClassAccessModifier>internal</NSwagClientClassAccessModifier>
  <NSwagGenerateClientInterfaces>true</NSwagGenerateClientInterfaces>
</OpenApiReference>
```

# [Kiota](#tab/kiota)

The available command-line switches for Kiota are described [here](https://learn.microsoft.com/openapi/kiota/using#client-generation).

At the time of writing, Kiota provides [no official integration](https://github.com/microsoft/kiota/issues/3005) with MSBuild.
Our [example project](https://github.com/json-api-dotnet/JsonApiDotNetCore/tree/master/src/Examples/OpenApiKiotaClientExample) takes a stab at it,
which seems to work. If you're an MSBuild expert, please help out!

```xml
<Target Name="ExcludeKiotaGeneratedCode" BeforeTargets="BeforeCompile;CoreCompile" Condition="$(DesignTimeBuild) != true And $(BuildingProject) == true">
  <ItemGroup>
    <Compile Remove="**\GeneratedCode\**\*.cs" />
  </ItemGroup>
</Target>

<Target Name="KiotaRunTool" BeforeTargets="BeforeCompile;CoreCompile" AfterTargets="ExcludeKiotaGeneratedCode"
  Condition="$(DesignTimeBuild) != true And $(BuildingProject) == true">
  <Exec
    Command="dotnet kiota generate --language CSharp --class-name ExampleApiClient --namespace-name OpenApiKiotaClientExample.GeneratedCode --output ./GeneratedCode --backing-store --exclude-backward-compatible --clean-output --clear-cache --log-level Error --openapi ../JsonApiDotNetCoreExample/GeneratedSwagger/JsonApiDotNetCoreExample.json" />
</Target>

<Target Name="IncludeKiotaGeneratedCode" BeforeTargets="BeforeCompile;CoreCompile" AfterTargets="KiotaRunTool"
  Condition="$(DesignTimeBuild) != true And $(BuildingProject) == true">
  <ItemGroup>
    <Compile Include="**\GeneratedCode\**\*.cs" />
  </ItemGroup>
</Target>
```

---

## Headers and caching

The use of HTTP headers varies per client generator. To use [ETags for caching](~/usage/caching.md), see the notes below.

# [NSwag](#tab/nswag)

To gain access to HTTP response headers, add the following in a `PropertyGroup` or directly in the `OpenApiReference`:

```
<NSwagWrapResponses>true</NSwagWrapResponses>
```

This enables the following code, which is explained below:

```c#
var getResponse = await ApiResponse.TranslateAsync(() => apiClient.GetPersonCollectionAsync());
string eTag = getResponse.Headers["ETag"].Single();
Console.WriteLine($"Retrieved {getResponse.Result?.Data.Count ?? 0} people.");

// wait some time...

getResponse = await ApiResponse.TranslateAsync(() => apiClient.GetPersonCollectionAsync(if_None_Match: eTag));

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

Use `HeadersInspectionHandlerOption` to gain access to HTTP response headers. For example:

```c#
var headerInspector = new HeadersInspectionHandlerOption
{
    InspectResponseHeaders = true
};

var responseDocument = await apiClient.Api.People.GetAsync(configuration => configuration.Options.Add(headerInspector));

string eTag = headerInspector.ResponseHeaders["ETag"].Single();
```

Due to a [bug in Kiota](https://github.com/microsoft/kiota/issues/4190), a try/catch block is needed additionally to make this work.

For a full example, see the [example project](https://github.com/json-api-dotnet/JsonApiDotNetCore/tree/master/src/Examples/OpenApiKiotaClientExample).

---

## Atomic operations

# [NSwag](#tab/nswag)

[Atomic operations](~/usage/writing/bulk-batch-operations.md) are fully supported.
The [example project](https://github.com/json-api-dotnet/JsonApiDotNetCore/tree/master/src/Examples/OpenApiNSwagClientExample)
demonstrates how to use them. It uses local IDs to:
- Create a new tag
- Create a new person
- Update the person to clear an attribute (using `TrackChangesFor`)
- Create a new todo-item, tagged with the new tag, and owned by the new person
- Assign the todo-item to the created person

# [Kiota](#tab/kiota)

[Atomic operations](~/usage/writing/bulk-batch-operations.md) are fully supported.
See the [example project](https://github.com/json-api-dotnet/JsonApiDotNetCore/tree/master/src/Examples/OpenApiKiotaClientExample)
demonstrates how to use them. It uses local IDs to:
- Create a new tag
- Create a new person
- Update the person to clear an attribute (using built-in backing-store)
- Create a new todo-item, tagged with the new tag, and owned by the new person
- Assign the todo-item to the created person

---

## Known limitations

# [NSwag](#tab/nswag)

| Limitation | Workaround | Links |
| --- | --- | --- |
| Partial POST/PATCH sends incorrect request | Use `TrackChangesFor` from `JsonApiDotNetCore.OpenApi.Client.NSwag` package | |
| Exception thrown on successful HTTP status | Use `TranslateAsync` from `JsonApiDotNetCore.OpenApi.Client.NSwag` package | https://github.com/RicoSuter/NSwag/issues/2499 |
| No `Accept` header sent when only error responses define `Content-Type` | `JsonApiDotNetCore.OpenApi.Swashbuckle` package contains workaround | |
| Schema type not always inferred with `allOf` | `JsonApiDotNetCore.OpenApi.Swashbuckle` package contains workaround | |
| Generated code for JSON:API extensions does not compile | `JsonApiDotNetCore.OpenApi.Swashbuckle` package contains workaround | |
| A project can't contain both JSON:API clients and regular OpenAPI clients | Use separate projects | |

# [Kiota](#tab/kiota)

| Limitation | Workaround | Links |
| --- | --- | --- |
| Properties are always nullable | - | https://github.com/microsoft/kiota/issues/3911 |
| JSON:API query strings are inaccessible | Use `SetQueryStringHttpMessageHandler.CreateScope` from `JsonApiDotNetCore.OpenApi.Client.Kiota` package | https://github.com/microsoft/kiota/issues/3800 |
| HTTP 304 (Not Modified) is not properly recognized | Catch `ApiException` and inspect the response status code | https://github.com/microsoft/kiota/issues/4190, https://github.com/microsoft/kiota-dotnet/issues/531 |
| No `Accept` header sent when only error responses define `Content-Type` | `JsonApiDotNetCore.OpenApi.Swashbuckle` package contains workaround | https://github.com/microsoft/kiota/issues/6572 |
| Generator warns about unsupported formats | Use `JsonApiDotNetCore.OpenApi.Client.Kiota` package | https://github.com/microsoft/kiota/issues/4227 |
| `Stream` response for HEAD request | - | https://github.com/microsoft/kiota/issues/4245 |
| Unhelpful exception messages | - | https://github.com/microsoft/kiota/issues/4349 |
| Discriminator properties aren't being set automatically | - |  https://github.com/microsoft/kiota/issues/4618 |
| Discriminator mappings must be repeated in every derived type used in responses | `JsonApiDotNetCore.OpenApi.Swashbuckle` package contains workaround | https://github.com/microsoft/kiota/issues/2432 |
| `x-abstract` in `openapi.json` is ignored | - | |
| No MSBuild / IDE support | Use `KiotaReference` from `JsonApiDotNetCore.OpenApi.Client.Kiota` package | https://github.com/microsoft/kiota/issues/3005 |
| Incorrect nullability in API methods | Use `KiotaReference` from `JsonApiDotNetCore.OpenApi.Client.Kiota` package | https://github.com/microsoft/kiota/issues/3944 |
| Generated code for JSON:API extensions does not compile | `JsonApiDotNetCore.OpenApi.Swashbuckle` package contains workaround | |
| Properties are always sent in alphabetic order | - | https://github.com/microsoft/kiota/issues/4680 |

---
