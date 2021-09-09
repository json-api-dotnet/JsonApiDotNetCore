# OpenAPI Generated Client

Given an OpenAPI specification, you can generate a client library using [NSwag](http://stevetalkscode.co.uk/openapireference-commands). We provide additional methods to enrich the features of this client.

## Installation

You need to install the following NuGet packages:

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


## OpenApiReference

Add a reference to your OpenAPI specification in your project file as demonstrated below.

```xml
<ItemGroup>
 <OpenApiReference Include="swagger.json">
   <Namespace>YourApplication.GeneratedCode</Namespace>
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
using System;
using ApiConsumer.GeneratedCode;

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
