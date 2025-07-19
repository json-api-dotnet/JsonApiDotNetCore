# Links

The code [here](https://github.com/json-api-dotnet/JsonApiDotNetCore/tree/master/test/JsonApiDotNetCoreTests/IntegrationTests/Links) shows various ways to configure which links are returned, and how they appear in responses.

> [!TIP]
> By default, absolute links are returned. To return relative links, set [JsonApiOptions.UseRelativeLinks](~/usage/options.md#relative-links) at startup.

> [!TIP]
> To add a global prefix to all routes, set `JsonApiOptions.Namespace` at startup.

Which links to render can be configured globally in options, then overridden per resource type, and then overridden per relationship.

- The `PhotoLocation` resource type turns off `TopLevelLinks` and `ResourceLinks`, and sets `RelationshipLinks` to `Related`.
- The `PhotoLocation.Album` relationship turns off all links for this relationship.

The various tests set `JsonApiOptions.Namespace` and `JsonApiOptions.UseRelativeLinks` to verify that the proper links are rendered.
This can't be set in the tests directly for technical reasons, so they use different `Startup` classes to control this.

Link rendering is fully controlled using attributes on your models. No further code is needed.
