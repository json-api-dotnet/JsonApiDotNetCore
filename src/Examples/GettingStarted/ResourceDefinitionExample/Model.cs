using JsonApiDotNetCore.Models;

namespace GettingStarted.ResourceDefinitionExample
{
    public sealed class Model : Identifiable
    {
        [Attr]
        public string DoNotExpose { get; set; }
    }
}
