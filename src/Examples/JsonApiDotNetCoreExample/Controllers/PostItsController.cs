using JsonApiDotNetCore.Controllers.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace JsonApiDotNetCoreExample.Controllers;

[ApiController]
[DisableRoutingConvention]
[Route("api/v1/postIts")]
public partial class PostItsController
{
}
