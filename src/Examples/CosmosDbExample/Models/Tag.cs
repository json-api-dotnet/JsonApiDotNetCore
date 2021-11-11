using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace CosmosDbExample.Models
{
    /// <summary>
    /// Represents a tag that can be assigned to a <see cref="TodoItem" />. In this example project, tags are owned by the <see cref="TodoItem" /> instances
    /// and do not represent separate entities.
    /// </summary>
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Tag
    {
        /// <summary>
        /// Gets or sets the name of the <see cref="Tag" />.
        /// </summary>
        [MinLength(1)]
        public string Name { get; set; } = null!;
    }
}
