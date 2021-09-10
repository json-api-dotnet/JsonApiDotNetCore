# Client Generator

You can you can generate a client library from an OpenAPI specification that describes a JsonApiDotNetCore application. For clients genearted with using [NSwag](http://stevetalkscode.co.uk/openapireference-commands) we provide an additional package that enables partial write requests.

## Installation

You are required to install the following NuGet packages:

- `JsonApiDotNetCore.OpenApiClient`
- `NSwag.ApiDescription.Client`
- `Microsoft.Extensions.ApiDescription.Cient`
- `NSwag.ApiDescription.Client`

The following examples demonstrate how to install the `JsonApiDotNetCore.OpenApiClient` package.

### CLI

```
dotnet add package JsonApiDotNetCore.OpenApiClient
```

### Visual Studio

```powershell
Install-Package JsonApiDotNetCore.OpenApiClient
```

### *.csproj

```xml
<ItemGroup>
  <!-- Be sure to check NuGet for the latest version # -->
  <PackageReference Include="JsonApiDotNetCore.OpenApiClient" Version="4.0.0" />
</ItemGroup>
```


## Adding an OpenApiReference

Add a reference to your OpenAPI specification in your project file as demonstrated below.

```xml
<ItemGroup>
 <OpenApiReference Include="swagger.json">
   <Namespace>ApiConsumer.GeneratedCode</Namespace>
   <ClassName>OpenApiClient</ClassName>
   <CodeGenerator>NSwagCSharp</CodeGenerator>
   <Options>/UseBaseUrl:false /GenerateClientInterfaces:true</Options>
 </OpenApiReference>
</ItemGroup>
```


## Usage

The NSwag tooling generates the OpenAPI client during a prebuild step. Once your application is built,
you can instantiate it using the class name as indicated in the project file.

```c#
namespace ApiConsumer
{
    class Program
    {
        static void Main(string[] args)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                OpenApiClient openApiClient = new OpenApiClient(httpClient);

                // IntelliSense is now available on `openApiClient`!
            }
        }
    }
}
```

Support for partial write requests can be enabled by leveraging the extensibility points of the generated client.

```c#
// Note that this class should be namespace in which NSwag generates the client. 
namespace ApiConsumer.GeneratedCode
{
    public partial class OpenApiClient : JsonApiClient
    {
        partial void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
        {
            SetSerializerSettingsForJsonApi(settings);
        }
    }
}
```

You can now perform a write request by calling the `RegisterAttributesForRequest` method. Calling this method treats all attributes that contain their default value (<c>null</c> for reference types, <c>0</c> for integers, <c>false</c> for booleans, etc) as omitted unless explicitly listed to include them using the `alwaysIncludedAttributeSelectors` parameter.

```c#
// Program.cs
static void Main(string[] args)
{
    using (HttpClient httpClient = new HttpClient())
    {
        OpenApiClient openApiClient = new OpenApiClient(httpClient);

        var requestDocument = new ApiResourcePatchRequestDocument
        {
            Data = new ApiResourceDataInPatchRequest
            {
                Id = 543,
                Type = ApiResourceResourceType.Airplanes,
                Attributes = new ApiResourceAttributesInPatchRequest
                {
                    someNullableAttribute = "Value"
                }
            }
        };

        using (apiClient.RegisterAttributesForRequestDocument<ApiResourcePatchRequestDocument, ApiResourceDataInPatchRequest>(requestDocument, apiResource => apiResource.AnotherNullableAttribute)
        {
            await apiClient.PatchApiResourceAsync(543, requestDocument));

            // The request will look like this:
            //
            // {
            //   "data": {
            //     "type": "apiResource",
            //     "id": "543",
            //     "attributes": {
            //       "someNullableAttribute": "Value",
            //       "anotherNullableAttribute": null,
            //     }
            //   }
            // }
        }

    }
}
```

