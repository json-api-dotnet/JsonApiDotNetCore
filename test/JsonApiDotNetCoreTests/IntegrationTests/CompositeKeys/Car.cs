using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.CompositeKeys;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.CompositeKeys")]
public sealed class Car : Identifiable<string?>
{
    [NotMapped]
    public override string? Id
    {
        get => RegionId == default && LicensePlate == default ? null : $"{RegionId}:{LicensePlate}";
        set
        {
            if (value == null)
            {
                RegionId = default;
                LicensePlate = default;
                return;
            }

            string[] elements = value.Split(':');

            if (elements.Length == 2 && long.TryParse(elements[0], out long regionId))
            {
                RegionId = regionId;
                LicensePlate = elements[1];
            }
            else
            {
                throw new InvalidOperationException($"Failed to convert ID '{value}'.");
            }
        }
    }

    [Attr]
    public string? LicensePlate { get; set; }

    [Attr]
    public long RegionId { get; set; }

    [HasOne]
    public Engine Engine { get; set; } = null!;

    [HasOne]
    public Dealership? Dealership { get; set; }
}
