# Hosting in Internet Information Services (IIS)

The code [here](https://github.com/json-api-dotnet/JsonApiDotNetCore/tree/master/test/JsonApiDotNetCoreTests/IntegrationTests/HostingInIIS) calls [UsePathBase](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.builder.usepathbaseextensions.usepathbase) to simulate hosting in IIS.
For details on how `UsePathBase` works, see [Understanding PathBase in ASP.NET Core](https://andrewlock.net/understanding-pathbase-in-aspnetcore/).

- At startup, the line `app.UsePathBase("/iis-application-virtual-directory")` configures ASP.NET to use the base path.
- `PaintingsController` uses a custom route to demonstrate that both features can be used together.
