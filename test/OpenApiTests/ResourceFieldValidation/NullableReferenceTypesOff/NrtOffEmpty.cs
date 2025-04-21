#nullable disable

using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.ResourceFieldValidation.NullableReferenceTypesOff;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
[Resource(PublicName = "empties", ControllerNamespace = "OpenApiTests.ResourceFieldValidation")]
public class NrtOffEmpty : Identifiable<int>;
