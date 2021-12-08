using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Blobs;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.Blobs")]
public sealed class ImageContainer : Identifiable<long>
{
    [Attr]
    public string FileName { get; set; } = null!;

    [Attr]
    public byte[] Data { get; set; } = Array.Empty<byte>();

    [Attr]
    public byte[]? Thumbnail { get; set; }
}
