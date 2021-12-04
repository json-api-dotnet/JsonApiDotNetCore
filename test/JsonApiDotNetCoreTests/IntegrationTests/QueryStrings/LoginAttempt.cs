using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class LoginAttempt : Identifiable<int>
{
    [Attr]
    public DateTimeOffset TriedAt { get; set; }

    [Attr]
    public bool IsSucceeded { get; set; }
}
