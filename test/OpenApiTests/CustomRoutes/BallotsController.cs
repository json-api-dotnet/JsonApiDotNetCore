using JsonApiDotNetCore.Controllers.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace OpenApiTests.CustomRoutes;

[DisableRoutingConvention]
[Route("voting-api/votes")]
partial class BallotsController;
