using System.ComponentModel.DataAnnotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExample.Models
{
    public class Tag : Identifiable
    {
        [Attr]
        [RegularExpression(@"^\W$")]
        public string Name { get; set; }

        [Attr]
        public TagColor Color { get; set; }
    }

    public enum TagColor
    {
        Red,
        Green,
        Blue
    }
}
