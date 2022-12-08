using System.ComponentModel.DataAnnotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExample.Models;

[Resource]
public class PostIt : Identifiable<int>
{
    [Attr]
    [Required]
    public bool Actif { get; set; }

    [Attr]
    [Required]
    public DateTime Date { get; set; }

    [Attr]
    [MaxLength(2, ErrorMessage = "Error")]
    public string Message { get; set; }

    [Attr]
    [MaxLength(2)]
    public string OtherMessage { get; set; }

    [Attr]
    [Required]
    public string RefOperateur { get; set; }
}
