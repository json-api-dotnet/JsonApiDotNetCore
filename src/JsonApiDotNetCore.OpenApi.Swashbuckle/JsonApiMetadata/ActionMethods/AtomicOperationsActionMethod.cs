using JsonApiDotNetCore.Controllers;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiMetadata.ActionMethods;

/// <summary>
/// The built-in JSON:API operations action method <see cref="BaseJsonApiOperationsController.PostOperationsAsync" />.
/// </summary>
internal sealed class AtomicOperationsActionMethod(Type controllerType)
    : BuiltinJsonApiActionMethod(controllerType);
