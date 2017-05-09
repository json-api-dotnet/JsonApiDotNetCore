---
currentMenu: installation
---

# Installation

- Visual Studio
```
Install-Package JsonApiDotnetCore
```

- *.csproj
```xml
<ItemGroup>
    <!-- Be sure to check NuGet for the latest version # -->
    <PackageReference Include="JsonApiDotNetCore" Version="2.0.1" />
</ItemGroup>
```

- CLI
```
$ dotnet add package jsonapidotnetcore
```

Click [here](https://www.nuget.org/packages/JsonApiDotnetCore/) for the latest NuGet version.

For pre-releases (develop branch), add the [MyGet](https://www.myget.org/feed/Details/research-institute) package feed 
(https://www.myget.org/F/research-institute/api/v3/index.json) 
to your nuget configuration.