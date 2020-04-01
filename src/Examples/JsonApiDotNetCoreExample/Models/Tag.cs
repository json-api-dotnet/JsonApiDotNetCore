using System.ComponentModel.DataAnnotations;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCoreExample.Models
{
    public class Tag : Identifiable
    {
        [Attr]
        [MaxLength(15)]
        public string Name { get; set; }
    }
}
