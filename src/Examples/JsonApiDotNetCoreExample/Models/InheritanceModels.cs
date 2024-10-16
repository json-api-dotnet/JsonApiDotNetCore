using System.ComponentModel.DataAnnotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

#pragma warning disable AV1507 // File contains multiple types

namespace JsonApiDotNetCoreExample.Models;
/*
[Resource]
public abstract class Building : Identifiable<long>
{
    [Attr]
    [Required]
    public int? SurfaceInSquareMeters { get; set; }
}

[Resource]
public sealed class Bunker : Building
{
    [Attr]
    [Required]
    public int? EmbrasureCount { get; set; }
}
[Resource]
public sealed class Shed : Building;
[Resource]
public sealed class Skyscraper : Building;

[Resource]
public abstract class Residence : Building
{
    [Attr]
    public int NumberOfResidents { get; set; }
}

[Resource]
public sealed class Studio : Residence;
[Resource]
public sealed class Apartment : Residence;
[Resource]
public sealed class Manor : Residence;
[Resource]
public sealed class Castle : Residence;

[Resource]
public abstract class Window : Identifiable<Guid>
{
    [Attr]
    public bool IsShielded { get; set; }
}

[Resource]
public sealed class TiltWindow : Window
{
    [Attr]
    public bool IsOpen { get; set; }
}
*/
