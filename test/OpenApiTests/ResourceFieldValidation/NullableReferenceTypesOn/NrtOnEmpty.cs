using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.ResourceFieldValidation.NullableReferenceTypesOn;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
[Resource(PublicName = "empties", ControllerNamespace = "OpenApiTests.ResourceFieldValidation")]
public class NrtOnEmpty : Identifiable<int>
{
}
