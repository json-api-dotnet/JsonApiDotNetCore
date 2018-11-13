# Installation

Click [here](https://www.nuget.org/packages/JsonApiDotnetCore/) for the latest NuGet version.

```
dotnet add package JsonApiDotnetCore
```

```powershell
Install-Package JsonApiDotnetCore
```

```xml
<ItemGroup>
  <!-- Be sure to check NuGet for the latest version # -->
  <PackageReference Include="JsonApiDotNetCore" Version="3.0.0" />
</ItemGroup>
```

## Pre-Release Packages

Occasionally we will release experimental features as pre-release packages on our
MyGet feed. You can download these by adding [the pacakge feed](https://www.myget.org/feed/Details/research-institute) to your nuget configuration.

These releases are deployed from the `develop` branch directly.

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="JADNC Pre-Release" value="https://www.myget.org/F/research-institute/api/v3/index.json" />
  </packageSources>
</configuration>
```