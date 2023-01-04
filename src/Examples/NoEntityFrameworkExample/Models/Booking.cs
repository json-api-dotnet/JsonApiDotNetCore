using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace NoEntityFrameworkExample.Models;

// This is the main resource for which I'm creating the API:
[Resource]
public class Booking : Identifiable<string>
{
    [Attr(PublicName = "title")]
    public required string Title { get; set; }

    [HasMany(PublicName = "spaces")]
    public required List<Space> Spaces { get; set; } // space ids
}
