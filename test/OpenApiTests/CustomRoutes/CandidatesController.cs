using JsonApiDotNetCore.Controllers.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace OpenApiTests.CustomRoutes;

[DisableRoutingConvention]
[Route("voting-api/contenders")]
partial class CandidatesController;
