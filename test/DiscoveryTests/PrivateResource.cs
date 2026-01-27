using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace DiscoveryTests;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource]
public sealed class PrivateResource : Identifiable<long>;
