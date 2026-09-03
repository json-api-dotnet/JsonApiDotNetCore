using JsonApiDotNetCore.Controllers.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace JsonApiDotNetCoreTests.IntegrationTests.Endpoints.JsonApiControllers.CustomRoutes;

[DisableRoutingConvention]
[Route("world-civilians")]
partial class CiviliansController;
