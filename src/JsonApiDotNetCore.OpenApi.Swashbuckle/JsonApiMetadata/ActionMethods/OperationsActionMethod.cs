using JsonApiDotNetCore.Controllers;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata.ActionMethods;

/// <summary>
/// The built-in JSON:API atomic:operations action method <see cref="BaseJsonApiOperationsController.PostOperationsAsync" />.
/// </summary>
internal sealed class OperationsActionMethod(Type controllerType)
    : JsonApiActionMethod(controllerType);
