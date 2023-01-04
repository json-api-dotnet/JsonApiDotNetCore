using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace NoEntityFrameworkExample.Models;

// This is another resource which should be connected to Bookings.
[Resource]
public class Space : Identifiable<string>
{
    [Attr(PublicName = "title")]
    public required string Title { get; set; }
}
