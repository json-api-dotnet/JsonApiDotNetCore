# Alternate Routes

The code [here](https://github.com/json-api-dotnet/JsonApiDotNetCore/tree/master/test/JsonApiDotNetCoreTests/IntegrationTests/CustomRoutes) shows how the default JSON:API routes can be changed.

The classes `TownsController` and `CiviliansController`:
- Are decorated with `[DisableRoutingConvention]` to turn off the default JSON:API routing convention.
- Are decorated with the ASP.NET `[Route]` attribute to specify at which route the controller is exposed.
- Are augmented with non-standard JSON:API action methods, whose `[HttpGet]` attributes specify a custom route.
