---
name: Bug report
about: Create a report to help us improve
title: ''
labels: ''
assignees: ''

---

_Please read our [Contributing Guides](https://github.com/json-api-dotnet/JsonApiDotNetCore/blob/master/.github/CONTRIBUTING.md) before submitting a bug._

#### DESCRIPTION
_A clear and concise description of what the bug is._

#### STEPS TO REPRODUCE
_Consider to include your code here, such as models, DbContext, controllers, resource services, repositories, resource definitions etc. Please also include the request URL with body (if applicable) and the full exception stack trace (set `options.IncludeExceptionStackTraceInErrors` to `true`) in case of errors._ It may also be helpful to include the produced SQL, which can be made visible in logs by adding this to appsettings.json:

```json
{
  "Logging": {
    "LogLevel": {
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

1.
2.
3.

#### EXPECTED BEHAVIOR
_A clear and concise description of what you expected to happen._

#### ACTUAL BEHAVIOR
_A clear and concise description of what happens instead._

#### VERSIONS USED
- JsonApiDotNetCore version:
- ASP.NET Core version:
- Entity Framework Core version:
- Database provider:
