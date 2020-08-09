using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Models.Annotation;

namespace JsonApiDotNetCoreExample.Models
{
    /// <summary>
    /// This is to test HasManyThrough, using non standard property names
    /// </summary>
    public class PlayerAward
    {
        [Key]
        [Required]
        public short AwardNo { get; set; }
        [HasOne]
        [ForeignKey(nameof(AwardNo))]
        public Award Award { get; set; }
        [Key]
        [Required]
        public Guid RecipientNo { get; set; }
        [ForeignKey(nameof(RecipientNo))]
        [HasOne]
        public Player Recipient { get; set; }
    }
}
